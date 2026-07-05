from __future__ import annotations

import argparse
import json
import re
import subprocess
from collections import Counter
from pathlib import Path


DEFAULT_ASSEMBLY = (
    r"F:\SteamLibrary\steamapps\common\Library Of Ruina"
    r"\LibraryOfRuina_Data\Managed\Assembly-CSharp.dll"
)
DEFAULT_DNSPY = (
    r"C:\Users\User\Documents\library_of_ruina_mod開發"
    r"\tools\dnSpy-netframework\dnSpy.Console.exe"
)


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Extract rough ability/passive effect summaries from decompiled Library of Ruina C# classes."
    )
    parser.add_argument(
        "--static-knowledge",
        default=str(Path("generated") / "static_knowledge"),
        help="Directory containing cards.json, passives.json, and key_pages.json.",
    )
    parser.add_argument(
        "--assembly",
        default=DEFAULT_ASSEMBLY,
        help="Path to Assembly-CSharp.dll.",
    )
    parser.add_argument(
        "--dnspy",
        default=DEFAULT_DNSPY,
        help="Path to dnSpy.Console.exe.",
    )
    parser.add_argument(
        "--decompiled",
        default=str(Path("generated") / "decompiled" / "Assembly-CSharp" / "Assembly-CSharp"),
        help="Directory containing decompiled Assembly-CSharp .cs files.",
    )
    parser.add_argument(
        "--out",
        default=str(Path("generated") / "static_knowledge"),
        help="Output directory.",
    )
    parser.add_argument(
        "--skip-decompile",
        action="store_true",
        help="Do not invoke dnSpy even if the decompiled folder is missing.",
    )
    args = parser.parse_args()

    static_knowledge = Path(args.static_knowledge)
    decompiled_dir = Path(args.decompiled)
    out_dir = Path(args.out)
    out_dir.mkdir(parents=True, exist_ok=True)

    if not decompiled_dir.exists() and not args.skip_decompile:
        decompile_assembly(Path(args.dnspy), Path(args.assembly), decompiled_dir.parent)

    cards = read_json(static_knowledge / "cards.json")
    passives = read_json(static_knowledge / "passives.json")
    key_pages = read_json(static_knowledge / "key_pages.json")
    emotion_cards = read_json_if_exists(static_knowledge / "emotion_cards.json")
    emotion_egos = read_json_if_exists(static_knowledge / "emotion_egos.json")

    card_abilities = export_card_abilities(cards, decompiled_dir)
    passive_effects = export_passive_effects(passives, key_pages, decompiled_dir)
    emotion_card_effects = export_emotion_card_effects(emotion_cards, decompiled_dir)
    key_page_profiles = build_key_page_profiles(key_pages, passive_effects)
    emotion_card_profiles = build_emotion_card_profiles(emotion_cards, emotion_card_effects)
    ego_page_profiles = build_ego_page_profiles(emotion_egos, cards, card_abilities)

    write_json(out_dir / "ability_effects.json", card_abilities)
    write_json(out_dir / "passive_effects.json", passive_effects)
    write_json(out_dir / "emotion_card_effects.json", emotion_card_effects)
    write_json(out_dir / "key_page_profiles.json", key_page_profiles)
    write_json(out_dir / "emotion_card_profiles.json", emotion_card_profiles)
    write_json(out_dir / "ego_page_profiles.json", ego_page_profiles)
    write_effect_summary(
        out_dir / "ability_effect_summary.md",
        card_abilities,
        passive_effects,
        key_page_profiles,
        emotion_card_effects,
        emotion_card_profiles,
        ego_page_profiles,
    )

    print("Ability export complete.")
    print(f"  card ability scripts: {len(card_abilities)}")
    print(f"  passive abilities: {len(passive_effects)}")
    print(f"  emotion card abilities: {len(emotion_card_effects)}")
    print(f"  key page profiles: {len(key_page_profiles)}")
    print(f"  emotion card profiles: {len(emotion_card_profiles)}")
    print(f"  ego page profiles: {len(ego_page_profiles)}")
    print(f"  out: {out_dir.resolve()}")
    return 0


def decompile_assembly(dnspy: Path, assembly: Path, out_parent: Path) -> None:
    out_parent.mkdir(parents=True, exist_ok=True)
    command = [
        str(dnspy),
        "-o",
        str(out_parent),
        "--no-sln",
        "--no-resources",
        "--no-resx",
        "--no-baml",
        str(assembly),
    ]
    subprocess.run(command, check=True)


def export_card_abilities(cards: list[dict], decompiled_dir: Path) -> list[dict]:
    script_ids = sorted(
        {
            script
            for card in cards
            for script in card.get("abilityScripts", card.get("scripts", []))
            if script
        }
    )
    records = []
    for script_id in script_ids:
        class_names = [
            f"DiceCardAbility_{script_id}",
            f"DiceCardSelfAbility_{script_id}",
        ]
        classes = []
        for class_name in class_names:
            source = read_class_source(decompiled_dir, class_name)
            if source is None:
                continue
            classes.append(analyze_class(class_name, source, "card"))

        effects = [effect for c in classes for effect in c["effects"]]
        records.append(
            {
                "scriptId": script_id,
                "classes": classes,
                "found": bool(classes),
                "effectTypes": sorted({effect["type"] for effect in effects}),
                "triggers": sorted({trigger for c in classes for trigger in c["triggers"]}),
                "profileTags": derive_profile_tags(effects),
            }
        )
    return records


def export_passive_effects(passives: list[dict], key_pages: list[dict], decompiled_dir: Path) -> list[dict]:
    passive_ids = {
        passive.get("id")
        for passive in passives
        if passive.get("id") is not None
    }
    for key_page in key_pages:
        for passive_ref in key_page.get("passives", []):
            if passive_ref.get("id") is not None:
                passive_ids.add(passive_ref.get("id"))

    passive_by_id = {passive.get("id"): passive for passive in passives}
    records = []
    for passive_id in sorted(passive_ids):
        class_name = f"PassiveAbility_{passive_id}"
        source = read_class_source(decompiled_dir, class_name)
        analysis = analyze_class(class_name, source, "passive") if source is not None else None
        passive_info = passive_by_id.get(passive_id, {})
        records.append(
            {
                "passiveId": passive_id,
                "className": class_name,
                "found": source is not None,
                "level": passive_info.get("level"),
                "rarity": passive_info.get("rarity"),
                "cost": passive_info.get("cost"),
                "debugDesc": analysis.get("debugDesc") if analysis else None,
                "triggers": analysis.get("triggers") if analysis else [],
                "effects": analysis.get("effects") if analysis else [],
                "effectTypes": sorted({effect["type"] for effect in analysis.get("effects", [])}) if analysis else [],
                "profileTags": derive_profile_tags(analysis.get("effects", []) if analysis else []),
                "methodSummaries": analysis.get("methodSummaries") if analysis else [],
            }
        )
    return records


def export_emotion_card_effects(emotion_cards: list[dict], decompiled_dir: Path) -> list[dict]:
    script_ids = sorted(
        {
            card.get("script")
            for card in emotion_cards
            if card.get("script")
        }
    )
    records = []
    for script_id in script_ids:
        class_name = f"EmotionCardAbility_{script_id}"
        source = read_class_source(decompiled_dir, class_name)
        analysis = analyze_class(class_name, source, "emotion_card") if source is not None else None
        records.append(
            {
                "scriptId": script_id,
                "className": class_name,
                "found": source is not None,
                "debugDesc": analysis.get("debugDesc") if analysis else None,
                "triggers": analysis.get("triggers") if analysis else [],
                "effects": analysis.get("effects") if analysis else [],
                "effectTypes": sorted({effect["type"] for effect in analysis.get("effects", [])}) if analysis else [],
                "profileTags": derive_profile_tags(analysis.get("effects", []) if analysis else []),
            }
        )
    return records


def build_key_page_profiles(key_pages: list[dict], passive_effects: list[dict]) -> list[dict]:
    passive_by_id = {effect["passiveId"]: effect for effect in passive_effects}
    profiles = []
    for key_page in key_pages:
        passive_ids = [ref.get("id") for ref in key_page.get("passives", []) if ref.get("id") is not None]
        passive_summaries = [passive_by_id[passive_id] for passive_id in passive_ids if passive_id in passive_by_id]
        profile_tags = sorted(
            {
                tag
                for passive in passive_summaries
                for tag in passive.get("profileTags", [])
            }
        )
        effect_types = sorted(
            {
                effect_type
                for passive in passive_summaries
                for effect_type in passive.get("effectTypes", [])
            }
        )
        profiles.append(
            {
                "keyPageId": key_page.get("id"),
                "name": key_page.get("name"),
                "hp": key_page.get("hp"),
                "stagger": key_page.get("stagger"),
                "speedMin": key_page.get("speedMin"),
                "speedMax": key_page.get("speedMax"),
                "passiveIds": passive_ids,
                "effectTypes": effect_types,
                "profileTags": profile_tags,
                "passiveSummaries": [
                    {
                        "passiveId": passive.get("passiveId"),
                        "debugDesc": passive.get("debugDesc"),
                        "triggers": passive.get("triggers"),
                        "effectTypes": passive.get("effectTypes"),
                        "profileTags": passive.get("profileTags"),
                        "methodSummaries": passive.get("methodSummaries", []),
                    }
                    for passive in passive_summaries
                ],
                "sourceFiles": key_page.get("sourceFiles", []),
            }
        )
    return profiles


def build_emotion_card_profiles(emotion_cards: list[dict], emotion_card_effects: list[dict]) -> list[dict]:
    effect_by_script = {effect["scriptId"]: effect for effect in emotion_card_effects}
    profiles = []
    for emotion_card in emotion_cards:
        effect_summary = effect_by_script.get(emotion_card.get("script"))
        profiles.append(
            {
                "emotionCardId": emotion_card.get("id"),
                "name": emotion_card.get("name"),
                "state": emotion_card.get("state"),
                "sephirah": emotion_card.get("sephirah"),
                "level": emotion_card.get("level"),
                "emotionLevel": emotion_card.get("emotionLevel"),
                "emotionRate": emotion_card.get("emotionRate"),
                "targetType": emotion_card.get("targetType"),
                "script": emotion_card.get("script"),
                "found": bool(effect_summary and effect_summary.get("found")),
                "triggers": effect_summary.get("triggers", []) if effect_summary else [],
                "effectTypes": effect_summary.get("effectTypes", []) if effect_summary else [],
                "profileTags": effect_summary.get("profileTags", []) if effect_summary else [],
                "effects": effect_summary.get("effects", []) if effect_summary else [],
                "sourceFiles": emotion_card.get("sourceFiles", []),
            }
        )
    return profiles


def build_ego_page_profiles(
    emotion_egos: list[dict],
    cards: list[dict],
    card_abilities: list[dict],
) -> list[dict]:
    card_by_id = {card.get("id"): card for card in cards}
    ability_by_script = {ability.get("scriptId"): ability for ability in card_abilities}
    profiles = []
    for emotion_ego in emotion_egos:
        card = card_by_id.get(emotion_ego.get("cardId"))
        ability_scripts = card.get("abilityScripts", card.get("scripts", [])) if card else []
        ability_summaries = [
            ability_by_script[script]
            for script in ability_scripts
            if script in ability_by_script
        ]
        effect_types = sorted(
            {
                effect_type
                for ability in ability_summaries
                for effect_type in ability.get("effectTypes", [])
            }
        )
        profile_tags = sorted(
            {
                tag
                for ability in ability_summaries
                for tag in ability.get("profileTags", derive_profile_tags(flatten_effects(ability)))
            }
        )
        profiles.append(
            {
                "egoId": emotion_ego.get("id"),
                "sephirah": emotion_ego.get("sephirah"),
                "cardId": emotion_ego.get("cardId"),
                "cardFound": card is not None,
                "card": build_ego_card_summary(card) if card else None,
                "effectTypes": effect_types,
                "profileTags": profile_tags,
                "abilitySummaries": [
                    {
                        "scriptId": ability.get("scriptId"),
                        "found": ability.get("found"),
                        "triggers": ability.get("triggers"),
                        "effectTypes": ability.get("effectTypes"),
                    }
                    for ability in ability_summaries
                ],
                "sourceFiles": emotion_ego.get("sourceFiles", []),
            }
        )
    return profiles


def build_ego_card_summary(card: dict) -> dict:
    return {
        "name": card.get("name"),
        "cost": card.get("cost"),
        "range": card.get("range"),
        "rarity": card.get("rarity"),
        "diceProfile": build_card_dice_profile(card.get("behaviours") or []),
        "abilityScripts": card.get("abilityScripts", card.get("scripts", [])),
        "actionScripts": card.get("actionScripts", []),
        "sourceFiles": card.get("sourceFiles", []),
    }


def build_card_dice_profile(behaviours: list[dict]) -> dict:
    attack = []
    defense = []
    evade = []
    details = Counter()
    for behaviour in behaviours:
        average = dice_average(behaviour)
        detail = behaviour.get("detail")
        if detail:
            details[detail] += 1
        if behaviour.get("type") == "Atk":
            attack.append(average)
        elif detail == "Evasion":
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


def flatten_effects(ability: dict) -> list[dict]:
    return [
        effect
        for class_summary in ability.get("classes", [])
        for effect in class_summary.get("effects", [])
    ]


def analyze_class(class_name: str, source: str, ability_kind: str) -> dict:
    return {
        "className": class_name,
        "abilityKind": ability_kind,
        "debugDesc": extract_debug_desc(source),
        "triggers": extract_override_methods(source),
        "effects": extract_effects(source),
        "methodSummaries": extract_method_summaries(source),
    }


def read_class_source(decompiled_dir: Path, class_name: str) -> str | None:
    path = decompiled_dir / f"{class_name}.cs"
    if not path.exists():
        return None
    return path.read_text(encoding="utf-8-sig", errors="replace")


def extract_debug_desc(source: str) -> str | None:
    match = re.search(r"public override string debugDesc.*?return \"(?P<desc>.*?)\";", source, re.S)
    if match:
        return unescape_csharp_string(match.group("desc"))
    return None


def extract_override_methods(source: str) -> list[str]:
    return sorted(set(re.findall(r"public override [^{;]+ (?P<name>\w+)\s*\(", source)))


def extract_method_summaries(source: str) -> list[dict]:
    summaries = []
    int_arrays = extract_int_arrays(source)
    keyword_arrays = extract_keyword_arrays(source)
    method_pattern = re.compile(
        r"public override (?P<returnType>[\w<>\[\].]+)\s+"
        r"(?P<name>\w+)\s*\((?P<parameters>[^)]*)\)\s*\{"
    )
    for match in method_pattern.finditer(source):
        body = extract_braced_block(source, match.end() - 1)
        if body is None:
            continue
        returns = extract_returns(body)
        conditions = extract_conditions(body)
        dice_stat_bonuses = extract_dice_stat_bonuses(body)
        keyword_aliases = extract_keyword_aliases(body, keyword_arrays)
        keyword_buf_applications = extract_keyword_buf_applications(body, keyword_arrays | keyword_aliases)
        summary = {
            "name": match.group("name"),
            "returnType": match.group("returnType"),
            "parameters": normalize_code(match.group("parameters")),
            "conditions": conditions,
            "conditionHints": derive_condition_hints(conditions, body),
            "returns": returns,
            "numericHints": derive_numeric_hints(returns, dice_stat_bonuses, keyword_buf_applications),
            "diceStatBonuses": dice_stat_bonuses,
            "keywordBufApplications": keyword_buf_applications,
            "cardIdChecks": extract_card_id_checks(body, int_arrays),
            "usesRandom": "RandomUtil." in body,
        }
        summaries.append(summary)
    return summaries


def extract_int_arrays(source: str) -> dict[str, list[int]]:
    arrays = {}
    pattern = re.compile(
        r"(?:public|private|protected)\s+int\[\]\s+(?P<name>\w+)\s*=\s*new int\[\]\s*\{(?P<body>.*?)\};",
        re.S,
    )
    for match in pattern.finditer(source):
        values = [int(value) for value in re.findall(r"-?\d+", match.group("body"))]
        arrays[match.group("name")] = values
    return arrays


def extract_keyword_arrays(source: str) -> dict[str, list[str]]:
    arrays = {}
    pattern = re.compile(
        r"(?:public|private|protected)\s+KeywordBuf\[\]\s+(?P<name>\w+)\s*=\s*new KeywordBuf\[\]\s*\{(?P<body>.*?)\};",
        re.S,
    )
    for match in pattern.finditer(source):
        arrays[match.group("name")] = re.findall(r"KeywordBuf\.(\w+)", match.group("body"))
    return arrays


def extract_keyword_aliases(body: str, keyword_arrays: dict[str, list[str]]) -> dict[str, list[str]]:
    aliases = {}
    pattern = re.compile(
        r"KeywordBuf\s+(?P<var>\w+)\s*=\s*RandomUtil\.SelectOne<KeywordBuf>\s*\(\s*(?:this\.)?(?P<array>\w+)\s*\)",
        re.S,
    )
    for match in pattern.finditer(body):
        values = keyword_arrays.get(match.group("array"))
        if values:
            aliases[match.group("var")] = values
    return aliases


def extract_braced_block(source: str, open_brace_index: int) -> str | None:
    if open_brace_index < 0 or open_brace_index >= len(source) or source[open_brace_index] != "{":
        return None
    depth = 0
    for index in range(open_brace_index, len(source)):
        char = source[index]
        if char == "{":
            depth += 1
        elif char == "}":
            depth -= 1
            if depth == 0:
                return source[open_brace_index + 1:index]
    return None


def extract_conditions(body: str) -> list[dict]:
    conditions = []
    index = 0
    while True:
        match = re.search(r"\bif\s*\(", body[index:])
        if match is None:
            break
        condition_start = index + match.end() - 1
        condition = extract_parenthesized(body, condition_start)
        if condition is None:
            index = condition_start + 1
            continue
        text, end_index = condition
        conditions.append(
            {
                "text": normalize_code(text),
                "hints": classify_condition(text),
            }
        )
        index = end_index + 1
    return dedupe_dicts(conditions)


def extract_parenthesized(source: str, open_paren_index: int) -> tuple[str, int] | None:
    if open_paren_index < 0 or open_paren_index >= len(source) or source[open_paren_index] != "(":
        return None
    depth = 0
    for index in range(open_paren_index, len(source)):
        char = source[index]
        if char == "(":
            depth += 1
        elif char == ")":
            depth -= 1
            if depth == 0:
                return source[open_paren_index + 1:index], index
    return None


def classify_condition(condition: str) -> list[str]:
    text = normalize_code(condition)
    hints = set()
    if "behavior.Detail ==" in text:
        detail_match = re.search(r"BehaviourDetail\.(\w+)", text)
        hints.add(f"dice_detail={detail_match.group(1)}" if detail_match else "dice_detail_condition")
    if "GetID()" in text:
        hints.add("card_id_condition")
    if ".IsDead()" in text:
        hints.add("death_state_condition")
    if "owner.IsBreakLifeZero()" in text or "breakDetail" in text:
        hints.add("stagger_state_condition")
    if "PlayPoint" in text or "playPoint" in text:
        hints.add("light_condition")
    if "bufListDetail" in text or "GetKewordBufStack" in text or "GetActivatedBufList" in text:
        hints.add("status_condition")
    if "emotionDetail" in text or "Emotion" in text:
        hints.add("emotion_condition")
    if "RandomUtil" in text:
        hints.add("random_condition")
    if "Count" in text:
        hints.add("count_condition")
    return sorted(hints)


def extract_returns(body: str) -> list[dict]:
    returns = []
    for match in re.finditer(r"\breturn\s+(?P<expr>[^;]+);", body):
        expression = normalize_code(match.group("expr"))
        returns.append(
            {
                "expression": expression,
                "kind": classify_expression(expression),
                "numericValue": parse_numeric_constant(expression),
            }
        )
    return dedupe_dicts(returns)


def classify_expression(expression: str) -> str:
    if parse_numeric_constant(expression) is not None:
        return "constant"
    if any(operator in expression for operator in ["+", "-", "*", "/"]):
        return "formula"
    if expression.startswith("base."):
        return "base_call"
    if expression in {"true", "false", "null"}:
        return "literal"
    return "symbolic"


def parse_numeric_constant(expression: str) -> int | float | None:
    text = expression.strip()
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    if re.fullmatch(r"-?\d+(?:\.\d+)?f?", text):
        return float(text.rstrip("f"))
    return None


def extract_dice_stat_bonuses(body: str) -> list[dict]:
    bonuses = []
    for block_match in re.finditer(r"new DiceStatBonus\s*\{(?P<body>.*?)\}", body, re.S):
        block = block_match.group("body")
        fields = {}
        for field, expression in re.findall(r"(\w+)\s*=\s*([^,\n}]+)", block):
            fields[field] = normalize_code(expression)
        if fields:
            bonuses.append(fields)
    return dedupe_dicts(bonuses)


def extract_keyword_buf_applications(body: str, keyword_arrays: dict[str, list[str]]) -> list[dict]:
    applications = []
    pattern = re.compile(
        r"AddKeywordBuf(?:ThisRound)?By(?:Card|Etc)\s*\(\s*(?P<keyword>[^,]+)\s*,\s*(?P<amount>[^,\)]+)",
        re.S,
    )
    for match in pattern.finditer(body):
        keyword_expression = normalize_code(match.group("keyword"))
        amount = normalize_code(match.group("amount"))
        applications.append(
            {
                "keyword": parse_keyword_expression(keyword_expression),
                "keywordExpression": keyword_expression,
                "possibleKeywords": resolve_keyword_expression(keyword_expression, keyword_arrays),
                "amountExpression": amount,
                "amountValue": parse_numeric_constant(amount),
                "thisRound": "AddKeywordBufThisRound" in match.group(0),
            }
        )
    return dedupe_dicts(applications)


def parse_keyword_expression(expression: str) -> str | None:
    match = re.fullmatch(r"KeywordBuf\.(\w+)", expression or "")
    if match:
        return match.group(1)
    return None


def resolve_keyword_expression(expression: str, keyword_arrays: dict[str, list[str]]) -> list[str]:
    if expression in keyword_arrays:
        return keyword_arrays[expression]
    parsed = parse_keyword_expression(expression)
    return [parsed] if parsed else []


def extract_card_id_checks(body: str, int_arrays: dict[str, list[int]]) -> list[dict]:
    checks = {}
    for match in re.finditer(r"GetID\s*\(\s*\)\s*==\s*(?P<expr>[^|&;\)\n]+)", body):
        expression = normalize_code(match.group("expr"))
        checks[expression] = resolve_int_expression(expression, int_arrays)
    for match in re.finditer(r"card\.GetID\s*\(\s*\)\s*==\s*(?P<expr>[^|&;\)\n]+)", body):
        expression = normalize_code(match.group("expr"))
        checks[expression] = resolve_int_expression(expression, int_arrays)
    return [
        {
            "expression": expression,
            "value": value,
        }
        for expression, value in sorted(checks.items())
    ]


def resolve_int_expression(expression: str, int_arrays: dict[str, list[int]]) -> int | None:
    numeric = parse_numeric_constant(expression)
    if isinstance(numeric, int):
        return numeric
    match = re.fullmatch(r"(?:this\.)?(?P<name>\w+)\[(?P<index>\d+)\]", expression or "")
    if not match:
        return None
    values = int_arrays.get(match.group("name"))
    if values is None:
        return None
    index = int(match.group("index"))
    if index >= len(values):
        return None
    return values[index]


def derive_condition_hints(conditions: list[dict], body: str) -> list[str]:
    hints = {
        hint
        for condition in conditions
        for hint in condition.get("hints", [])
    }
    if "RandomUtil." in body:
        hints.add("uses_random")
    if "BattleObjectManager.instance.GetAliveList" in body:
        hints.add("alive_unit_list")
    if "BattleObjectManager.instance.GetList" in body:
        hints.add("unit_list")
    return sorted(hints)


def derive_numeric_hints(
    returns: list[dict],
    dice_stat_bonuses: list[dict],
    keyword_buf_applications: list[dict],
) -> list[dict]:
    hints = []
    for return_value in returns:
        if return_value.get("numericValue") is not None or return_value.get("kind") == "formula":
            hints.append(
                {
                    "source": "return",
                    "expression": return_value.get("expression"),
                    "numericValue": return_value.get("numericValue"),
                }
            )
    for bonus in dice_stat_bonuses:
        for field, expression in bonus.items():
            hints.append(
                {
                    "source": "DiceStatBonus",
                    "field": field,
                    "expression": expression,
                    "numericValue": parse_numeric_constant(expression),
                }
            )
    for application in keyword_buf_applications:
        hints.append(
            {
                "source": "KeywordBuf",
                "keyword": application.get("keyword"),
                "expression": application.get("amountExpression"),
                "numericValue": application.get("amountValue"),
            }
        )
    return dedupe_dicts(hints)


def normalize_code(value: str | None) -> str | None:
    if value is None:
        return None
    return re.sub(r"\s+", " ", value).strip()


def dedupe_dicts(items: list[dict]) -> list[dict]:
    seen = set()
    deduped = []
    for item in items:
        key = json.dumps(item, sort_keys=True, ensure_ascii=False)
        if key in seen:
            continue
        seen.add(key)
        deduped.append(item)
    return deduped


def extract_effects(source: str) -> list[dict]:
    effects: list[dict] = []

    for match in re.finditer(r"AddKeywordBufBy(?:Card|Etc)\s*\(\s*KeywordBuf\.(?P<keyword>\w+)\s*,\s*(?P<amount>-?\d+)", source):
        effects.append(
            {
                "type": "add_keyword_buf",
                "keyword": match.group("keyword"),
                "amount": int(match.group("amount")),
                "target": infer_target(source, match.start()),
            }
        )

    for match in re.finditer(r"RecoverPlayPointByCard\s*\(\s*(?P<amount>-?\d+)\s*\)", source):
        effects.append({"type": "recover_light", "amount": int(match.group("amount")), "target": "owner"})

    for match in re.finditer(r"DrawCards\s*\(\s*(?P<amount>-?\d+)\s*\)", source):
        effects.append({"type": "draw_cards", "amount": int(match.group("amount")), "target": "owner"})

    for match in re.finditer(r"RecoverHP\s*\(\s*(?P<amount>-?\d+)\s*\)", source):
        effects.append({"type": "recover_hp", "amount": int(match.group("amount")), "target": infer_target(source, match.start())})

    for match in re.finditer(r"TakeBreakDamage\s*\(\s*(?P<amount>-?\d+)", source):
        effects.append({"type": "stagger_damage", "amount": int(match.group("amount")), "target": infer_target(source, match.start())})

    for match in re.finditer(r"SpeedDiceNumAdder\s*\(\).*?return\s+(?P<amount>-?\d+)\s*;", source, re.S):
        effects.append({"type": "speed_dice_num_adder", "amount": int(match.group("amount")), "target": "owner"})

    for match in re.finditer(r"GetSpeedDiceAdder\s*\(\).*?return\s+(?P<amount>-?\d+)\s*;", source, re.S):
        effects.append({"type": "speed_value_adder", "amount": int(match.group("amount")), "target": "owner"})

    if "BeforeAddToHand" in source:
        effects.append({"type": "deck_building_rule", "trigger": "BeforeAddToHand"})

    structural_markers = [
        ("BeforeRollDice", "dice_power_modifier", "BeforeRollDice"),
        ("ChangeAttackTarget", "targeting_modifier", "ChangeAttackTarget"),
        ("GetResistHP", "resistance_modifier", "GetResistHP"),
        ("GetResistBP", "resistance_modifier", "GetResistBP"),
        ("GetDamageReduction", "damage_reduction", "GetDamageReduction"),
        ("GetBreakDamageReduction", "damage_reduction", "GetBreakDamageReduction"),
        ("GetDamageIncreaseRate", "damage_increase", "GetDamageIncreaseRate"),
        ("GetBreakDamageIncreaseRate", "damage_increase", "GetBreakDamageIncreaseRate"),
        ("GetStartHp", "start_hp_modifier", "GetStartHp"),
        ("GetMinHp", "survival_floor", "GetMinHp"),
        ("OnTakeDamageByAttack", "on_take_damage_reaction", "OnTakeDamageByAttack"),
        ("OnGiveDamage", "damage_modifier", "OnGiveDamage"),
        ("BeforeGiveDamage", "damage_modifier", "BeforeGiveDamage"),
        ("GetDamageFactor", "damage_modifier", "GetDamageFactor"),
        ("ChangeCost", "cost_modifier", "ChangeCost"),
        ("SetCost", "cost_modifier", "SetCost"),
    ]
    for marker, effect_type, trigger in structural_markers:
        if marker in source:
            effects.append({"type": effect_type, "trigger": trigger})

    return dedupe_effects(effects)


def infer_target(source: str, index: int) -> str:
    window = source[max(0, index - 180):index + 180]
    if "battleUnitModel" in window:
        return "selected_unit_or_ally"
    if "base.card.target" in window or "card.target" in window or "target." in window:
        return "target"
    if "base.owner" in window or "this.owner" in window or "card.owner" in window:
        return "owner"
    if "BattleObjectManager.instance.GetAliveList" in window:
        return "allies_or_units"
    return "unknown"


def dedupe_effects(effects: list[dict]) -> list[dict]:
    seen = set()
    deduped = []
    for effect in effects:
        key = json.dumps(effect, sort_keys=True, ensure_ascii=False)
        if key in seen:
            continue
        seen.add(key)
        deduped.append(effect)
    return deduped


def derive_profile_tags(effects: list[dict]) -> list[str]:
    tags = set()
    for effect in effects:
        effect_type = effect.get("type")
        keyword = effect.get("keyword")
        if effect_type == "speed_dice_num_adder":
            tags.add("extra_speed_die")
        if effect_type == "speed_value_adder":
            tags.add("speed_bonus")
        if effect_type == "recover_light":
            tags.add("light_recovery")
        if effect_type == "draw_cards":
            tags.add("card_draw")
        if effect_type == "recover_hp":
            tags.add("survival")
        if effect_type == "stagger_damage":
            tags.add("stagger_pressure")
        if effect_type == "deck_building_rule":
            tags.add("deck_building_rule")
        if effect_type in ("dice_power_modifier", "damage_increase", "damage_modifier"):
            tags.add("power_modifier")
        if effect_type == "targeting_modifier":
            tags.add("targeting_modifier")
        if effect_type in ("resistance_modifier", "damage_reduction", "survival_floor", "on_take_damage_reaction"):
            tags.add("survival")
        if effect_type == "cost_modifier":
            tags.add("resource_modifier")
        if keyword:
            tags.add(keyword_to_profile_tag(keyword))
    return sorted(tags)


def keyword_to_profile_tag(keyword: str) -> str:
    mapping = {
        "Strength": "strength_gain",
        "Endurance": "endurance_gain",
        "Protection": "protection",
        "BreakProtection": "stagger_protection",
        "Bleeding": "bleed",
        "Burn": "burn",
        "Smoke": "smoke",
        "Paralysis": "paralysis",
        "Binding": "bind",
        "Vulnerable": "fragile",
        "Weak": "weak",
    }
    return mapping.get(keyword, f"keyword_{keyword}")


def read_json(path: Path):
    return json.loads(path.read_text(encoding="utf-8"))


def read_json_if_exists(path: Path):
    if not path.exists():
        return []
    return read_json(path)


def write_json(path: Path, data) -> None:
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def unescape_csharp_string(value: str) -> str:
    if "\\" not in value:
        return value
    return value.encode("utf-8").decode("unicode_escape")


def write_effect_summary(
    path: Path,
    card_abilities: list[dict],
    passive_effects: list[dict],
    key_page_profiles: list[dict],
    emotion_card_effects: list[dict],
    emotion_card_profiles: list[dict],
    ego_page_profiles: list[dict],
) -> None:
    card_found = [record for record in card_abilities if record["found"]]
    passive_found = [record for record in passive_effects if record["found"]]
    emotion_card_found = [record for record in emotion_card_effects if record["found"]]
    card_effect_types = Counter(effect_type for record in card_found for effect_type in record["effectTypes"])
    passive_effect_types = Counter(effect_type for record in passive_found for effect_type in record["effectTypes"])
    emotion_card_effect_types = Counter(
        effect_type
        for record in emotion_card_found
        for effect_type in record["effectTypes"]
    )
    passive_methods = [
        method
        for record in passive_found
        for method in record.get("methodSummaries", [])
    ]
    key_page_tags = Counter(tag for profile in key_page_profiles for tag in profile.get("profileTags", []))
    emotion_card_tags = Counter(tag for profile in emotion_card_profiles for tag in profile.get("profileTags", []))
    ego_page_tags = Counter(tag for profile in ego_page_profiles for tag in profile.get("profileTags", []))

    lines = [
        "# Ability Effect Summary",
        "",
        "Counts:",
        "",
        "```text",
        f"card ability scripts: {len(card_abilities)}",
        f"card ability scripts found: {len(card_found)}",
        f"passive abilities: {len(passive_effects)}",
        f"passive abilities found: {len(passive_found)}",
        f"passive methods summarized: {len(passive_methods)}",
        f"passive methods with conditions: {sum(1 for method in passive_methods if method.get('conditions'))}",
        f"passive methods with numeric hints: {sum(1 for method in passive_methods if method.get('numericHints'))}",
        f"passive methods with card id checks: {sum(1 for method in passive_methods if method.get('cardIdChecks'))}",
        f"emotion card abilities: {len(emotion_card_effects)}",
        f"emotion card abilities found: {len(emotion_card_found)}",
        f"key page profiles: {len(key_page_profiles)}",
        f"emotion card profiles: {len(emotion_card_profiles)}",
        f"ego page profiles: {len(ego_page_profiles)}",
        "```",
        "",
    ]
    append_counter(lines, "Card Effect Types", card_effect_types)
    append_counter(lines, "Passive Effect Types", passive_effect_types)
    append_counter(lines, "Emotion Card Effect Types", emotion_card_effect_types)
    append_counter(lines, "Key Page Profile Tags", key_page_tags)
    append_counter(lines, "Emotion Card Profile Tags", emotion_card_tags)
    append_counter(lines, "EGO Page Profile Tags", ego_page_tags)
    append_examples(lines, "Card Ability Examples", card_found[:30], "scriptId")
    append_examples(lines, "Passive Ability Examples", passive_found[:30], "passiveId")
    append_examples(lines, "Emotion Card Ability Examples", emotion_card_found[:30], "scriptId")

    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def append_counter(lines: list[str], title: str, counter: Counter) -> None:
    lines.extend([f"## {title}", "", "```text"])
    if not counter:
        lines.append("(none)")
    else:
        for key, count in counter.most_common():
            lines.append(f"{key}: {count}")
    lines.extend(["```", ""])


def append_examples(lines: list[str], title: str, records: list[dict], id_key: str) -> None:
    lines.extend([f"## {title}", "", "```text"])
    if not records:
        lines.append("(none)")
    else:
        for record in records:
            lines.append(
                "{0}={1} found={2} triggers={3} effects={4}".format(
                    id_key,
                    record.get(id_key),
                    record.get("found"),
                    ",".join(record.get("triggers", [])),
                    ",".join(record.get("effectTypes", [])),
                )
            )
    lines.extend(["```", ""])


if __name__ == "__main__":
    raise SystemExit(main())
