# Implementation Versions

This document defines the intended implementation roadmap.

Its purpose is to stop the strategy system from trying to implement every concept at once.

Each version should produce a useful, testable system before the next layer is added.

## V0 Current Working Prototype

Goal:

```text
Prove that the mod can load, patch original methods, assign cards, and advance battle automatically.
```

Status:

```text
Implemented.
```

Capabilities:

```text
Harmony patch installation.
Interception of BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx).
Automatic battle flow through StageController phases.
Basic card assignment through ActionExecutor.
Simple rule-based action selection.
```

Purpose:

```text
Validate the control pipeline before building a smarter strategy.
```

## V1 Legal Action + Local Evaluation

Goal:

```text
Replace direct hard-coded action choice with legal action enumeration and LocalActionEvaluation.
```

Planned components:

```text
BattleContextReader
AvailableResourceReader
LegalActionFinder
ActionCandidate
DiceProbabilityCalculator
LocalActionEvaluator
```

Current implementation status:

```text
Partially implemented.
Smoke-tested in game.
```

Implemented:

```text
ActionCandidate.
InteractionType.
LegalActionFinder.
LegalActionSearchReport.
LegalActionSearchResult.
LocalActionEvaluation.
LocalActionEvaluator.
DiceProbabilityCalculator.
DiceProbabilityProfile.
TacticalPlanner integration.
Selection logging through TryDicingLogger.
```

Smoke-test verified:

```text
AutoPlayPatch interception.
ActionCandidate generation.
LocalActionEvaluation scoring.
TacticalPlanner selection.
ActionExecutor execution.
tryDicing.log output.
No observed ERROR lines in the checked log sample.
```

Still planned:

```text
Dedicated BattleContextReader / AvailableResourceReader split.
More accurate damage and stagger probability.
Better threat scoring from enemy dice.
More detailed legality checks for special page types.
```

Supported scope:

```text
Normal melee pages.
Clash and OneSidedAttack.
Basic legality checks.
Basic dice min/max probability.
Rough expected HP damage.
Rough expected stagger damage.
First-version kill and stagger opportunity bonuses.
Basic overkill.
Light cost.
Basic card draw and light recovery value.
```

Out of scope:

```text
Full passive modeling.
Full card script modeling.
Mass attacks.
Counter dice.
Special stage mechanics.
Agent integration.
```

Strategy behavior:

```text
For each usable speed die, select the highest local-evaluation action.
```

## V1.5 Minimal BuildProfile

Goal:

```text
Let local evaluation understand a minimal version of player build intent.
```

Planned components:

```text
BuildProfile
BuildProfileInferer
Minimal UserBuildIntent override
ResolvedBuildProfile
KeyCardPolicy basics
MechanicTags basics
```

Purpose:

```text
Prevent V1 scoring from becoming only a damage or clash score.
```

## V2 Scene Plan Evaluation

Goal:

```text
Evaluate a full scene plan instead of selecting each speed die independently.
```

Planned components:

```text
CandidateBattlePlanGenerator
PlanEvaluator
ThreatCoverageEvaluator
AllyRiskManagementEvaluator
ResourceFutureEvaluator
RedundancyWasteEvaluator
OutcomeConfirmationEvaluator
ExchangeEvaluator
IrreversibleGainEvaluator
```

Supported behavior:

```text
Avoid team-wide overkill.
Prefer useful focus fire.
Handle dangerous enemy dice.
Protect or intentionally risk allies based on payoff.
Estimate whether resources remain playable next scene.
Avoid redundant low-value actions.
Prefer plans that create irreversible gains or prevent irreversible losses.
```

Strategy behavior:

```text
Generate several candidate plans.
Select the highest PlanEvaluation result.
```

## V3 UserBuildIntent + ResolvedBuildProfile

Goal:

```text
Give the player a way to express intended build behavior without letting the system rewrite the build.
```

Planned components:

```text
UserBuildIntent configuration.
BuildProfile override / merge rules.
ResolvedBuildProfile.
Key card policy overrides.
Natural-language notes for later agent use.
```

Purpose:

```text
Preserve the player's deckbuilding and build-expression role while allowing the strategy to play according to that intent.
```

## V4 FeasibleOutcome + Objective-Plan Pair

Goal:

```text
Derive scene objectives from feasible outcomes instead of abstract wishes.
```

Planned components:

```text
FeasibleOutcomeDeriver
SceneObjectiveHypothesisGenerator
ObjectivePlanPair
ObjectiveAwarePlanGenerator
ObjectiveAwarePlanEvaluator
```

Supported behavior:

```text
Derive likely kills.
Derive likely staggers.
Derive likely threat interceptions.
Derive likely resource recovery lines.
Derive likely setup progress.
Derive likely enemy-effect suppression.
Generate objective-plan pairs only when grounded in feasible outcomes.
```

## V4.5 Mock / Rule AgentDecisionProvider

Goal:

```text
Test the AgentDecisionProvider interface before connecting an external agent.
```

MockAgentDecisionProvider:

```text
Uses a fixed simple rule such as selecting the highest evaluated plan.
Its purpose is integration testing, not intelligence.
```

RuleAgentDecisionProvider:

```text
Uses local deterministic rules to choose among evaluated objective-plan pairs.
Its purpose is to provide a non-LLM baseline and fallback.
```

## V5 MCP Agent Integration

Goal:

```text
Allow an external agent to participate in tactical decision making through MCP-backed tools.
```

Planned components:

```text
Local Decision Bridge
MCP Server
AgentContextExporter
ExternalAgentDecisionProvider
AgentControlMode
PlanChoiceRequest
PlanChoiceResponse
VariantRequest
VariantResponse
PlanProposal
ProgramValidator
PlanReEvaluator
Timeout and fallback handling
```

Purpose:

```text
Let an agent choose among compressed, evaluated strategic alternatives using plan evaluations, exchange summaries and irreversible-gain estimates, without receiving raw game dumps or full candidate lists by default.
```

Agent control modes:

```text
Mode 1 Select:
  Agent can only select one program-generated, program-evaluated plan.

Mode 2 Request Variant:
  Agent can ask the program to generate a new plan variant in a specific direction.
  The agent still cannot directly edit actions.

Mode 3 Propose Plan:
  Agent can propose a full plan or partial actions.
  The proposal must be validated and re-evaluated before it can be executed.
```

Implementation order:

```text
Implement Mode 1 first.
Add Mode 2 after candidate variant generation is reliable.
Add Mode 3 only after proposal validation and plan re-evaluation are reliable.
```

## V6 Advanced LoR Mechanics + Learning

Goal:

```text
Gradually model complex Library of Ruina mechanics and learn from battle results.
```

Possible additions:

```text
Mass attacks.
Counter dice.
Full ranged rules.
E.G.O pages.
Abnormality pages.
Emotion coins and emotion levels.
Complex passive scripts.
Complex card scripts.
Special stage mechanics.
Battle result logging.
Weight tuning.
Stage-specific knowledge.
Build performance feedback.
```

## Dependency Chain

```text
V0 Current Working Prototype
  -> V1 Legal Action + Local Evaluation
  -> V1.5 Minimal BuildProfile
  -> V2 Scene Plan Evaluation
  -> V3 UserBuildIntent + ResolvedBuildProfile
  -> V4 FeasibleOutcome + Objective-Plan Pair
  -> V4.5 Mock / Rule AgentDecisionProvider
  -> V5 MCP Agent Integration
  -> V6 Advanced LoR Mechanics + Learning
```

Later design terms may be documented before they are implemented.

Implementation should still follow the version boundary unless there is a clear reason to change it.


