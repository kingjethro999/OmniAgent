"""
OmniAgent Engine — Task Router

Classifies incoming tasks as LOCAL or CLOUD based on multiple heuristics:
  - Keyword matching (privacy-sensitive terms -> local)
  - Token length estimation
  - Complexity scoring (reasoning depth, mathematical content, etc.)
"""

from __future__ import annotations
import re, math
from dataclasses import dataclass, field
from omniagent.config import get_config
from omniagent.events import AgentEvent, EventType, RoutingDecision, event_bus

_LOCAL_KW = {
    "summarize","summary","list","format","rewrite","paraphrase","translate",
    "draft","reply","notification","reminder","schedule","file","folder",
    "rename","move","copy","delete","organize","sort","count","grep",
    "search","find","local","private","sensitive","confidential","offline",
}
_CLOUD_KW = {
    "analyze","research","compare","evaluate","synthesize","explain why",
    "reason","prove","derive","theorem","hypothesis","calculate integral",
    "differential","equation","matrix","multi-step","complex","advanced",
    "comprehensive","code review","architecture","design pattern","refactor",
    "write essay","creative writing","strategy","business plan",
}

@dataclass
class RoutingResult:
    decision: RoutingDecision
    complexity_score: float
    reasoning: str
    matched_keywords: list[str] = field(default_factory=list)

class TaskRouter:
    def __init__(self, complexity_threshold: float | None = None):
        self.threshold = complexity_threshold or get_config().complexity_threshold

    def route(self, task: str) -> RoutingResult:
        scores, matched, reasons = [], [], []
        kw_s, kw_m, kw_r = self._kw(task); scores.append(kw_s); matched.extend(kw_m); reasons.append(kw_r)
        ln_s, ln_r = self._length(task); scores.append(ln_s); reasons.append(ln_r)
        st_s, st_r = self._struct(task); scores.append(st_s); reasons.append(st_r)
        ma_s, ma_r = self._math(task); scores.append(ma_s); reasons.append(ma_r)
        w = [0.35, 0.20, 0.25, 0.20]
        cx = max(0.0, min(1.0, sum(s*wt for s,wt in zip(scores,w))))
        dec = RoutingDecision.CLOUD if cx >= self.threshold else RoutingDecision.LOCAL
        reasoning = " | ".join(reasons)
        event_bus.emit(AgentEvent(
            event_type=EventType.ROUTING,
            message=f"Routed to {dec.value} (score: {cx:.3f})",
            routing_decision=dec, complexity_score=cx,
            data={"reasoning": reasoning, "threshold": self.threshold},
        ))
        return RoutingResult(decision=dec, complexity_score=round(cx,3), reasoning=reasoning, matched_keywords=matched)

    def _kw(self, task):
        lo = task.lower()
        lh = [k for k in _LOCAL_KW if k in lo]
        ch = [k for k in _CLOUD_KW if k in lo]
        if not lh and not ch: return 0.5, [], "No keyword signals"
        cw = len(ch)*1.5; t = len(lh)+cw
        return (cw/t if t else 0.5), [f"+local:{k}" for k in lh]+[f"+cloud:{k}" for k in ch], f"KW: {len(lh)}L {len(ch)}C"

    def _length(self, task):
        wc = len(task.split())
        return 1.0/(1.0+math.exp(-0.03*(wc-80))), f"Len: {wc}w"

    def _struct(self, task):
        i = 0
        if re.search(r"\b(then|after that|next|finally|step \d)\b", task, re.I): i+=1
        if re.search(r"^\s*\d+[\.\)]\s", task, re.M): i+=1
        if re.search(r"\b(if|unless|otherwise|except|depending)\b", task, re.I): i+=1
        if re.search(r"\b(compare|versus|vs\.?|difference|pros and cons)\b", task, re.I): i+=1
        if task.count("?") >= 2: i+=1
        return min(1.0, i*0.25), f"Struct: {i}"

    def _math(self, task):
        i = 0
        if re.search(r"[∫∑∏√∞≈≠≤≥]", task): i+=2
        if re.search(r"\\(frac|sqrt|int|sum)\b", task): i+=2
        if re.search(r"\b[a-z]\s*[=<>]\s*\d", task, re.I): i+=1
        mt = {"integral","derivative","matrix","eigenvalue","polynomial","logarithm","theorem","proof"}
        if any(t in task.lower() for t in mt): i+=1
        return min(1.0, i*0.3), f"Math: {i}"
