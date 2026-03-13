using Newtonsoft.Json;

public class CombinedEvaluationInstance : IEvaluation
{
    #region Constructor
    private CombinedEvaluationInstance(List<Employee> evaluators, Employee? beingEvaluated)
    {
        Evaluators = evaluators ?? new();
        BeingEvaluated = beingEvaluated;

        entities = new();
        entitiesDict = new();
    }
    #endregion

    #region Private Fields
    [JsonProperty("Entities")]
    private List<CombinedEntityNode> entities;

    private ScoringOptions ScoringOptions => IsSystemEvaluation
        ? ConfigLoader.SystemEvaluationOptions.Scoring
        : ConfigLoader.EmployeeEvaluationOptions.Scoring;

    private Dictionary<string, CombinedEntityNode> entitiesDict;
    #endregion

    #region Public Props
    [JsonProperty("Evaluators")]
    public List<Employee> Evaluators { get; private set; }

    [JsonProperty("Being Evaluated")]
    public Employee? BeingEvaluated { get; private set; }
    #endregion

    #region Public Getonly Props
    [JsonIgnore]
    public bool IsCombined => true;

    [JsonIgnore]
    public bool IsSystemEvaluation => BeingEvaluated == null;

    [JsonIgnore]
    public Employee Evaluator => throw new NullReferenceException();

    [JsonIgnore]
    public List<CombinedEntityNode> Entities => entities;

    [JsonIgnore]
    public IReadOnlyDictionary<string, CombinedEntityNode> EntitiesDict => entitiesDict;
    
    [JsonIgnore]
    public IReadOnlyList<IEntityNode> ReadonlyEntities => entities;

    [JsonIgnore]
    public double TotalScore { get; private set; }

    [JsonIgnore]
    public string FinalNote => throw new NullReferenceException();

    [JsonIgnore]
    public bool RecommendAsTeamLead => throw new NullReferenceException();

    [JsonIgnore]
    public bool ReadyToBeAssistantTeamLeader => throw new NullReferenceException();
    #endregion

    #region Public Methods
    public IEntityNode? SearchFor(Predicate<IEntityNode> predicate)
    {
        return SearchFor(Entities, predicate);
    }
    public bool AssistantSectionEnabled()
    {
        throw new NullReferenceException();
    }
    public void CalculateTotalScore()
    {
        foreach (var entity in entities)
            entity.CalculateCombinedScore();

        var vars = new Dictionary<string, FormulaEngine.Value>
        {
            ["EntityScore"] = new FormulaEngine.Value(entities.Select(n => n.Score).ToList()),
            ["EntityWeight"] = new FormulaEngine.Value(entities.Select(n => n.Weight).ToList())
        };

        TotalScore = FormulaEngine.EvaluateToScalar(ScoringOptions.TotalFormula, vars, 0);
    }
    #endregion

    #region Private Methods
    private static IEntityNode? SearchFor(IReadOnlyList<IEntityNode> entities, Predicate<IEntityNode> predicate)
    {
        foreach (var entity in entities)
        {
            if (predicate(entity))
                return entity;

            if (entity.RootConfig.HasValue)
            {
                var found = SearchFor(entity.ReadonlyChilds ?? [], predicate);
                if (found != null)
                    return found;
            }
        }

        return null;
    }
    #endregion

    #region Public Static Methods
    public static CombinedEvaluationInstance GenerateCombinedEntity(List<EvaluationInstance> evals)
    {
        if (evals == null || evals.Count == 0)
            throw new ArgumentException("evals is null or empty.");

        var first = evals[0];

        bool sameBeingEvaluated = evals.All(e =>
            string.Equals(e.BeingEvaluated?.Code, first.BeingEvaluated?.Code, StringComparison.Ordinal));

        if (!sameBeingEvaluated)
            throw new InvalidOperationException("All evaluations must belong to the same BeingEvaluated.");

        bool sameMode = evals.All(e => e.IsSystemEvaluation == first.IsSystemEvaluation);
        if (!sameMode)
            throw new InvalidOperationException("All evaluations must have the same evaluation mode.");

        foreach (var eval in evals)
            eval.CalculateScore();

        var combined = new CombinedEvaluationInstance(
            evals.Select(e => e.Evaluator).ToList(),
            first.BeingEvaluated);

        var rootTemplates = GetUnionById(
            evals.SelectMany(e => e.Entities),
            x => x.ConfigEntityId);

        combined.entities = rootTemplates
            .Select(rootTemplate =>
            {
                var sameLevelRoots = evals
                    .Select(e => e.Entities.FirstOrDefault(x => x.ConfigEntityId == rootTemplate.ConfigEntityId))
                    .Where(x => x != null)
                    .Cast<EntityNode>()
                    .ToList();

                return BuildCombinedNode(rootTemplate, sameLevelRoots, combined.ScoringOptions);
            })
            .ToList();

        combined.entitiesDict = combined.entities.ToDictionary(o => o.ConfigEntityId, o => o);
        combined.CalculateTotalScore();

        return combined;
    }
    #endregion

    #region Private Static Methods
    private static List<T> GetUnionById<T>(IEnumerable<T> items, Func<T, string> idSelector)
    {
        var result = new List<T>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            var id = idSelector(item);
            if (seen.Add(id))
                result.Add(item);
        }

        return result;
    }
    private static CombinedEntityNode BuildCombinedNode(EntityNode template, 
        List<EntityNode> sameLevelEntities, ScoringOptions scoringOptions)
    {
        var node = new CombinedEntityNode(template, scoringOptions);

        if (node.ValueConfig.HasValue)
            node.SourceScores = sameLevelEntities.Select(x => x.Score).ToList();

        var allChildren = sameLevelEntities
            .Where(x => x.Childs != null)
            .SelectMany(x => x.Childs!)
            .ToList();

        if (allChildren.Count > 0)
        {
            var childTemplates = GetUnionById(allChildren, x => x.ConfigEntityId);

            node.Childs = new List<CombinedEntityNode>(childTemplates.Count);

            foreach (var childTemplate in childTemplates)
            {
                var matchingChildren = sameLevelEntities
                    .Select(parent => parent.Childs?.FirstOrDefault(c => c.ConfigEntityId == childTemplate.ConfigEntityId))
                    .Where(x => x != null)
                    .Cast<EntityNode>()
                    .ToList();

                node.Childs.Add(BuildCombinedNode(childTemplate, matchingChildren, scoringOptions));
            }

            node.ChildsDict = node.Childs.ToDictionary(x => x.ConfigEntityId, x => x);
        }
        return node;
    }
    #endregion
}