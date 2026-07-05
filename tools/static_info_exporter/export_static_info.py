from __future__ import annotations

import argparse
import json
from collections import Counter
from pathlib import Path
import xml.etree.ElementTree as ET


DEFAULT_STATIC_INFO = (
    r"F:\SteamLibrary\steamapps\common\Library Of Ruina"
    r"\LibraryOfRuina_Data\Managed\BaseMod\StaticInfo"
)


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Export Library of Ruina StaticInfo files into strategy-friendly JSON."
    )
    parser.add_argument(
        "--static-info",
        default=DEFAULT_STATIC_INFO,
        help="Path to LibraryOfRuina_Data/Managed/BaseMod/StaticInfo.",
    )
    parser.add_argument(
        "--out",
        default=str(Path("generated") / "static_knowledge"),
        help="Output directory inside the workspace.",
    )
    args = parser.parse_args()

    static_info = Path(args.static_info)
    out_dir = Path(args.out)
    out_dir.mkdir(parents=True, exist_ok=True)

    exporter = StaticInfoExporter(static_info)
    result = exporter.export()
    card_effect_profiles = build_card_effect_profiles(result.cards)
    write_json(out_dir / "cards.json", result.cards)
    write_json(out_dir / "card_effect_tags.json", card_effect_profiles)
    write_json(out_dir / "passives.json", result.passives)
    write_json(out_dir / "key_pages.json", result.key_pages)
    write_json(out_dir / "stages.json", result.stages)
    write_json(out_dir / "enemies.json", result.enemies)
    write_json(out_dir / "decks.json", result.decks)
    write_json(out_dir / "emotion_cards.json", result.emotion_cards)
    write_json(out_dir / "emotion_egos.json", result.emotion_egos)
    write_json(out_dir / "export_errors.json", result.errors)
    write_summary(out_dir / "summary.md", static_info, result)
    write_card_effect_summary(out_dir / "card_effect_summary.md", card_effect_profiles)

    print("Export complete.")
    print(f"  cards: {len(result.cards)}")
    print(f"  card_effect_tags: {len(card_effect_profiles)}")
    print(f"  passives: {len(result.passives)}")
    print(f"  key_pages: {len(result.key_pages)}")
    print(f"  stages: {len(result.stages)}")
    print(f"  enemies: {len(result.enemies)}")
    print(f"  decks: {len(result.decks)}")
    print(f"  emotion_cards: {len(result.emotion_cards)}")
    print(f"  emotion_egos: {len(result.emotion_egos)}")
    print(f"  errors: {len(result.errors)}")
    print(f"  out: {out_dir.resolve()}")
    return 0


class ExportResult:
    def __init__(self) -> None:
        self.cards: list[dict] = []
        self.passives: list[dict] = []
        self.key_pages: list[dict] = []
        self.stages: list[dict] = []
        self.enemies: list[dict] = []
        self.decks: list[dict] = []
        self.emotion_cards: list[dict] = []
        self.emotion_egos: list[dict] = []
        self.errors: list[dict] = []


class StaticInfoExporter:
    def __init__(self, static_info: Path) -> None:
        self.static_info = static_info
        self.errors: list[dict] = []

    def export(self) -> ExportResult:
        result = ExportResult()
        result.cards = self.export_folder("Card", "Card", parse_card)
        result.passives = self.export_folder("PassiveList", "Passive", parse_passive)
        result.key_pages = self.export_folder("EquipPage", "Book", parse_key_page)
        result.stages = self.export_folder("StageInfo", "Stage", parse_stage)
        result.enemies = self.export_folder("EnemyUnitInfo", "Enemy", parse_enemy)
        result.decks = self.export_folder("Deck", "Deck", parse_deck)
        result.emotion_cards = self.export_folder_all("EmotionCard", "EmotionCard", parse_emotion_card)
        result.emotion_egos = self.export_folder("EmotionEgo", "EmotionEgo", parse_emotion_ego)
        result.errors = self.errors
        return result

    def export_folder(self, folder_name: str, item_tag: str, parse_item) -> list[dict]:
        folder = self.static_info / folder_name
        records: dict[str, dict] = {}
        if not folder.exists():
            self.errors.append(
                {
                    "folder": folder_name,
                    "file": None,
                    "error": "folder not found",
                }
            )
            return []

        for path in sorted(folder.glob("*.txt")):
            try:
                root = ET.parse(path).getroot()
            except Exception as exc:
                self.errors.append(
                    {
                        "folder": folder_name,
                        "file": str(path),
                        "error": f"{type(exc).__name__}: {exc}",
                    }
                )
                continue

            for item in root.findall(item_tag):
                record = parse_item(item, path.name)
                record_id = str(record.get("id", ""))
                if not record_id:
                    continue

                if record_id in records:
                    records[record_id].setdefault("sourceFiles", [])
                    records[record_id]["sourceFiles"].append(path.name)
                else:
                    record["sourceFiles"] = [path.name]
                    records[record_id] = record

        return sorted(records.values(), key=lambda row: numeric_sort_key(row.get("id")))

    def export_folder_all(self, folder_name: str, item_tag: str, parse_item) -> list[dict]:
        folder = self.static_info / folder_name
        records: list[dict] = []
        if not folder.exists():
            self.errors.append(
                {
                    "folder": folder_name,
                    "file": None,
                    "error": "folder not found",
                }
            )
            return []

        for path in sorted(folder.glob("*.txt")):
            try:
                root = ET.parse(path).getroot()
            except Exception as exc:
                self.errors.append(
                    {
                        "folder": folder_name,
                        "file": str(path),
                        "error": f"{type(exc).__name__}: {exc}",
                    }
                )
                continue

            for item in root.findall(item_tag):
                record = parse_item(item, path.name)
                record["sourceFiles"] = [path.name]
                records.append(record)

        return sorted(
            records,
            key=lambda row: (
                "" if row.get("sephirah") is None else str(row.get("sephirah")),
                numeric_sort_key(row.get("id")),
                "" if row.get("script") is None else str(row.get("script")),
            ),
        )


def parse_card(node: ET.Element, source_file: str) -> dict:
    spec = node.find("Spec")
    behaviours = [parse_behaviour(child) for child in node.findall("./BehaviourList/Behaviour")]
    ability_scripts = collect_ability_scripts(node, behaviours)
    action_scripts = collect_action_scripts(behaviours)
    descriptions = collect_descriptions(node, behaviours)
    return {
        "id": attr_int(node, "ID"),
        "name": child_text(node, "Name"),
        "artwork": child_text(node, "Artwork"),
        "rarity": child_text(node, "Rarity"),
        "option": child_text(node, "Option"),
        "range": attr_text(spec, "Range"),
        "cost": attr_int(spec, "Cost"),
        "affection": attr_text(spec, "affection"),
        "chapter": child_int(node, "Chapter"),
        "priority": child_int(node, "Priority"),
        "script": child_text(node, "Script"),
        "scriptDesc": child_text(node, "ScriptDesc"),
        "behaviours": behaviours,
        "abilityScripts": ability_scripts,
        "actionScripts": action_scripts,
        "scripts": ability_scripts,
        "descriptions": descriptions,
        "tags": infer_tags(ability_scripts, descriptions),
    }


def parse_behaviour(node: ET.Element) -> dict:
    return {
        "min": attr_int(node, "Min"),
        "max": attr_int(node, "Dice"),
        "type": attr_text(node, "Type"),
        "detail": attr_text(node, "Detail"),
        "motion": attr_text(node, "Motion"),
        "effectRes": attr_text(node, "EffectRes"),
        "script": attr_text(node, "Script"),
        "actionScript": attr_text(node, "ActionScript"),
        "desc": attr_text(node, "Desc"),
    }


def parse_passive(node: ET.Element, source_file: str) -> dict:
    return {
        "id": attr_int(node, "ID"),
        "level": child_int(node, "Level"),
        "rarity": child_text(node, "Rarity"),
        "cost": child_int(node, "Cost"),
        "rawChildren": children_as_dict(node),
    }


def parse_key_page(node: ET.Element, source_file: str) -> dict:
    effect = node.find("EquipEffect")
    return {
        "id": attr_int(node, "ID"),
        "name": child_text(node, "Name"),
        "textId": child_int(node, "TextId"),
        "option": child_text(node, "Option"),
        "rarity": child_text(node, "Rarity"),
        "chapter": child_int(node, "Chapter"),
        "episode": child_int(node, "Episode"),
        "rangeType": child_text(node, "RangeType"),
        "bookIcon": child_text(node, "BookIcon"),
        "characterSkin": child_text(node, "CharacterSkin"),
        "hp": child_int(effect, "HP"),
        "stagger": child_int(effect, "Break"),
        "speedMin": child_int(effect, "SpeedMin"),
        "speedMax": child_int(effect, "Speed"),
        "passiveCost": child_int(effect, "PassiveCost"),
        "passives": parse_passive_refs(effect),
        "resistances": {
            "slashHp": child_text(effect, "SResist"),
            "pierceHp": child_text(effect, "PResist"),
            "bluntHp": child_text(effect, "HResist"),
            "slashStagger": child_text(effect, "SBResist"),
            "pierceStagger": child_text(effect, "PBResist"),
            "bluntStagger": child_text(effect, "HBResist"),
        },
    }


def parse_stage(node: ET.Element, source_file: str) -> dict:
    waves = []
    for wave in node.findall("Wave"):
        waves.append(
            {
                "formation": child_int(wave, "Formation"),
                "units": [to_int(child.text) for child in wave.findall("Unit")],
            }
        )

    invitation = node.find("Invitation")
    return {
        "id": attr_int(node, "id"),
        "name": child_text(node, "Name"),
        "chapter": child_int(node, "Chapter"),
        "episode": child_int(node, "Episode"),
        "storyType": child_text(node, "StoryType"),
        "floorNum": child_int(node, "FloorNum"),
        "invitation": {
            "combine": attr_text(invitation, "Combine"),
            "books": [to_int(child.text) for child in safe_findall(invitation, "Book")],
        },
        "conditions": [to_int(child.text) for child in node.findall("./Condition/Stage")],
        "waves": waves,
    }


def parse_enemy(node: ET.Element, source_file: str) -> dict:
    drop_tables = []
    for table in node.findall("DropTable"):
        drop_tables.append(
            {
                "level": attr_text(table, "Level"),
                "items": [
                    {
                        "bookId": to_int(item.text),
                        "prob": attr_text(item, "Prob"),
                    }
                    for item in table.findall("DropItem")
                ],
            }
        )

    return {
        "id": attr_int(node, "ID"),
        "nameId": child_int(node, "NameID"),
        "bookId": child_int(node, "BookId"),
        "deckId": child_int(node, "DeckId"),
        "exp": child_int(node, "Exp"),
        "height": {
            "min": child_int(node, "MinHeight"),
            "max": child_int(node, "MaxHeight"),
        },
        "dropTables": drop_tables,
    }


def parse_deck(node: ET.Element, source_file: str) -> dict:
    card_ids = [to_int(child.text) for child in node.findall("Card")]
    counts = Counter(card_ids)
    return {
        "id": attr_int(node, "ID"),
        "cards": card_ids,
        "cardCounts": [
            {"cardId": card_id, "count": count}
            for card_id, count in sorted(counts.items(), key=lambda pair: numeric_sort_key(pair[0]))
        ],
    }


def parse_emotion_card(node: ET.Element, source_file: str) -> dict:
    return {
        "id": attr_int(node, "ID"),
        "name": child_text(node, "Name"),
        "state": child_text(node, "State"),
        "sephirah": child_text(node, "Sephirah"),
        "level": child_int(node, "Level"),
        "emotionLevel": child_int(node, "EmotionLevel"),
        "emotionRate": child_int(node, "EmotionRate"),
        "targetType": child_text(node, "TargetType"),
        "script": child_text(node, "Script"),
        "rawChildren": children_as_dict(node),
    }


def parse_emotion_ego(node: ET.Element, source_file: str) -> dict:
    return {
        "id": attr_int(node, "ID"),
        "sephirah": child_text(node, "Sephirah"),
        "cardId": child_int(node, "Card"),
        "lockInBattle": child_text(node, "LockInBattle"),
        "rawChildren": children_as_dict(node),
    }


def parse_passive_refs(effect: ET.Element | None) -> list[dict]:
    refs = []
    if effect is None:
        return refs

    for node in effect.findall("Passive"):
        refs.append(
            {
                "id": to_int(node.text),
                "level": attr_int(node, "Level"),
            }
        )
    return refs


def collect_ability_scripts(card_node: ET.Element, behaviours: list[dict]) -> list[str]:
    scripts = []
    main_script = child_text(card_node, "Script")
    if main_script:
        scripts.append(main_script)

    for behaviour in behaviours:
        value = behaviour.get("script")
        if value:
            scripts.append(value)

    return sorted(set(scripts))


def collect_action_scripts(behaviours: list[dict]) -> list[str]:
    scripts = []
    for behaviour in behaviours:
        value = behaviour.get("actionScript")
        if value:
            scripts.append(value)

    return sorted(set(scripts))


def collect_descriptions(card_node: ET.Element, behaviours: list[dict]) -> list[str]:
    descriptions = []
    script_desc = child_text(card_node, "ScriptDesc")
    if script_desc:
        descriptions.append(script_desc)

    for behaviour in behaviours:
        desc = behaviour.get("desc")
        if desc:
            descriptions.append(desc)

    return descriptions


def infer_tags(scripts: list[str], descriptions: list[str]) -> list[str]:
    text = " ".join(scripts + descriptions).lower()
    tags = set()

    rules = [
        ("draw", ("draw", "carddraw", "카드", "책장")),
        ("recover_light", ("energy", "recoverlight", "light", "빛")),
        ("recover_hp", ("recoverhp", "heal", "hp", "체력")),
        ("stagger_damage", ("break", "breakdamage", "stagger", "흐트러짐")),
        ("bleed", ("bleed", "bleeding")),
        ("burn", ("burn", "fire", "화상")),
        ("smoke", ("smoke", "연기")),
        ("charge", ("charge",)),
        ("power_gain", ("power", "strength", "pw")),
        ("cost_limit", ("limit",)),
        ("weak", ("weak", "허약")),
        ("fragile", ("fragile", "취약")),
        ("paralysis", ("paralysis", "마비")),
        ("bind", ("bind", "속박")),
        ("protection", ("protection", "보호")),
    ]

    for tag, needles in rules:
        if any(needle in text for needle in needles):
            tags.add(tag)

    return sorted(tags)


def build_card_effect_profiles(cards: list[dict]) -> list[dict]:
    profiles = []
    for card in cards:
        dice_profile = build_dice_profile(card.get("behaviours") or [])
        all_tags = sorted(set(card.get("tags") or []))
        profiles.append(
            {
                "cardId": card.get("id"),
                "name": card.get("name"),
                "cost": card.get("cost"),
                "range": card.get("range"),
                "rarity": card.get("rarity"),
                "chapter": card.get("chapter"),
                "priority": card.get("priority"),
                "diceProfile": dice_profile,
                "effectTags": select_tags(
                    all_tags,
                    {
                        "bleed",
                        "burn",
                        "smoke",
                        "charge",
                        "weak",
                        "fragile",
                        "paralysis",
                        "bind",
                        "protection",
                    },
                ),
                "resourceTags": select_tags(all_tags, {"draw", "recover_light", "recover_hp"}),
                "setupTags": derive_setup_tags(all_tags),
                "riskTags": select_tags(all_tags, {"cost_limit"}),
                "allTags": all_tags,
                "abilityScripts": card.get("abilityScripts") or card.get("scripts") or [],
                "actionScripts": card.get("actionScripts") or [],
                "scripts": card.get("abilityScripts") or card.get("scripts") or [],
                "sourceFiles": card.get("sourceFiles") or [],
            }
        )

    return profiles


def build_dice_profile(behaviours: list[dict]) -> dict:
    attack = []
    defense = []
    evade = []
    details = Counter()

    for behaviour in behaviours:
        average = dice_average(behaviour)
        detail = behaviour.get("detail")
        if detail:
            details[detail] += 1

        die_type = behaviour.get("type")
        if die_type == "Atk":
            attack.append(average)
        elif behaviour.get("detail") == "Evasion":
            evade.append(average)
        else:
            defense.append(average)

    return {
        "diceCount": len(behaviours),
        "attackCount": len(attack),
        "defenseCount": len(defense),
        "evadeCount": len(evade),
        "attackAverage": round(sum(attack), 2) if attack else 0,
        "defenseAverage": round(sum(defense), 2) if defense else 0,
        "evadeAverage": round(sum(evade), 2) if evade else 0,
        "totalAverage": round(sum(attack) + sum(defense) + sum(evade), 2),
        "details": dict(sorted(details.items())),
    }


def dice_average(behaviour: dict) -> float:
    minimum = behaviour.get("min")
    maximum = behaviour.get("max")
    if minimum is None or maximum is None:
        return 0
    return (minimum + maximum) * 0.5


def select_tags(tags: list[str], allowed: set[str]) -> list[str]:
    return sorted(tag for tag in tags if tag in allowed)


def derive_setup_tags(tags: list[str]) -> list[str]:
    setup_tags = set()
    if any(tag in tags for tag in ("bleed", "burn", "smoke", "charge", "weak", "fragile", "paralysis", "bind")):
        setup_tags.add("status_setup")
    if "power_gain" in tags:
        setup_tags.add("power_setup")
    if "stagger_damage" in tags:
        setup_tags.add("stagger_setup")
    return sorted(setup_tags)


def children_as_dict(node: ET.Element) -> dict:
    data = {}
    for child in node:
        value = normalize_text(child.text)
        if child.attrib:
            value = {"text": value, "attributes": dict(child.attrib)}

        if child.tag in data:
            if not isinstance(data[child.tag], list):
                data[child.tag] = [data[child.tag]]
            data[child.tag].append(value)
        else:
            data[child.tag] = value
    return data


def safe_findall(node: ET.Element | None, tag: str) -> list[ET.Element]:
    if node is None:
        return []
    return list(node.findall(tag))


def child_text(node: ET.Element | None, tag: str) -> str | None:
    if node is None:
        return None
    child = node.find(tag)
    if child is None:
        return None
    return normalize_text(child.text)


def child_int(node: ET.Element | None, tag: str) -> int | None:
    return to_int(child_text(node, tag))


def attr_text(node: ET.Element | None, name: str) -> str | None:
    if node is None:
        return None
    return normalize_text(node.attrib.get(name))


def attr_int(node: ET.Element | None, name: str) -> int | None:
    return to_int(attr_text(node, name))


def normalize_text(value: str | None) -> str | None:
    if value is None:
        return None
    value = value.strip()
    return value if value else None


def to_int(value) -> int | None:
    if value is None:
        return None
    try:
        return int(str(value).strip())
    except (TypeError, ValueError):
        return None


def numeric_sort_key(value) -> tuple[int, int | str]:
    if isinstance(value, int):
        return (0, value)
    try:
        return (0, int(str(value)))
    except (TypeError, ValueError):
        return (1, "" if value is None else str(value))


def write_json(path: Path, data) -> None:
    path.write_text(
        json.dumps(data, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


def write_summary(path: Path, static_info: Path, result: ExportResult) -> None:
    lines = [
        "# Static Knowledge Export Summary",
        "",
        "Source:",
        "",
        "```text",
        str(static_info),
        "```",
        "",
        "Counts:",
        "",
        "```text",
        f"cards: {len(result.cards)}",
        f"passives: {len(result.passives)}",
        f"key_pages: {len(result.key_pages)}",
        f"stages: {len(result.stages)}",
        f"enemies: {len(result.enemies)}",
        f"decks: {len(result.decks)}",
        f"emotion_cards: {len(result.emotion_cards)}",
        f"emotion_egos: {len(result.emotion_egos)}",
        f"errors: {len(result.errors)}",
        "```",
        "",
        "Output files:",
        "",
        "```text",
        "cards.json",
        "card_effect_tags.json",
        "card_effect_summary.md",
        "passives.json",
        "key_pages.json",
        "stages.json",
        "enemies.json",
        "decks.json",
        "emotion_cards.json",
        "emotion_egos.json",
        "export_errors.json",
        "```",
        "",
        "Notes:",
        "",
        "```text",
        "This export is static knowledge for strategy evaluation.",
        "It is not the final legality source.",
        "Runtime legality must still use original game methods.",
        "```",
    ]

    if result.errors:
        lines.extend(["", "Errors:", "", "```text"])
        for error in result.errors[:20]:
            lines.append(f"{error.get('folder')} | {error.get('file')} | {error.get('error')}")
        if len(result.errors) > 20:
            lines.append(f"... {len(result.errors) - 20} more")
        lines.append("```")

    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def write_card_effect_summary(path: Path, profiles: list[dict]) -> None:
    all_tags = Counter()
    effect_tags = Counter()
    resource_tags = Counter()
    setup_tags = Counter()
    risk_tags = Counter()
    cards_with_any_tag = 0

    for profile in profiles:
        if profile.get("allTags"):
            cards_with_any_tag += 1
        all_tags.update(profile.get("allTags") or [])
        effect_tags.update(profile.get("effectTags") or [])
        resource_tags.update(profile.get("resourceTags") or [])
        setup_tags.update(profile.get("setupTags") or [])
        risk_tags.update(profile.get("riskTags") or [])

    lines = [
        "# Card Effect Tag Summary",
        "",
        "Purpose:",
        "",
        "```text",
        "This file summarizes card_effect_tags.json.",
        "It is for checking whether static card tagging is useful before wiring it into C# strategy code.",
        "```",
        "",
        "Counts:",
        "",
        "```text",
        f"cards: {len(profiles)}",
        f"cardsWithAnyTag: {cards_with_any_tag}",
        "```",
        "",
    ]

    append_counter_section(lines, "All Tags", all_tags)
    append_counter_section(lines, "Effect Tags", effect_tags)
    append_counter_section(lines, "Resource Tags", resource_tags)
    append_counter_section(lines, "Setup Tags", setup_tags)
    append_counter_section(lines, "Risk Tags", risk_tags)
    append_examples(lines, profiles)

    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def append_counter_section(lines: list[str], title: str, counter: Counter) -> None:
    lines.extend([f"## {title}", "", "```text"])
    if not counter:
        lines.append("(none)")
    else:
        for tag, count in counter.most_common():
            lines.append(f"{tag}: {count}")
    lines.extend(["```", ""])


def append_examples(lines: list[str], profiles: list[dict]) -> None:
    interesting = [
        profile
        for profile in profiles
        if profile.get("resourceTags") or profile.get("setupTags") or profile.get("riskTags")
    ][:30]

    lines.extend(["## First Tagged Examples", "", "```text"])
    if not interesting:
        lines.append("(none)")
    else:
        for profile in interesting:
            lines.append(
                "cardId={0} cost={1} range={2} allTags={3} abilityScripts={4}".format(
                    profile.get("cardId"),
                    profile.get("cost"),
                    profile.get("range"),
                    ",".join(profile.get("allTags") or []),
                    ",".join(profile.get("abilityScripts") or profile.get("scripts") or []),
                )
            )
    lines.extend(["```", ""])


if __name__ == "__main__":
    raise SystemExit(main())
