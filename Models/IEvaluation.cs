public interface IEvaluation
{
    bool IsCombined { get; }
    bool IsSystemEvaluation { get; }

    Employee Evaluator { get; }
    Employee? BeingEvaluated { get; }

    IReadOnlyList<IEntityNode> ReadonlyEntities { get; }

    string FinalNote { get; }
    bool ReadyToBeAssistantTeamLeader { get; }
    bool RecommendAsTeamLead { get; }
    double TotalScore { get; }

    bool AssistantSectionEnabled();
    IEntityNode? SearchFor(Predicate<IEntityNode> condition);
}