using TermBullet.Application.Ai;

namespace TermBullet.Tests.Application.Ai;

public sealed class AiPlanningDraftValidatorTests
{
    [Fact]
    public void Validate_accepts_new_project_java_study_draft()
    {
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_project",
              "summary": "Java study roadmap.",
              "actions": [
                { "type": "create_tag", "name": "estudo-java" },
                {
                  "type": "create_note",
                  "tag": "estudo-java",
                  "content": "Java study roadmap",
                  "description": "Scope and study sequence."
                },
                {
                  "type": "create_task",
                  "tag": "estudo-java",
                  "collection": "today",
                  "content": "Install JDK and run Hello World",
                  "priority": "high"
                },
                {
                  "type": "create_task",
                  "tag": "estudo-java",
                  "collection": "week",
                  "content": "Study Java syntax"
                },
                {
                  "type": "create_task",
                  "tag": "estudo-java",
                  "collection": "backlog",
                  "content": "Learn Spring Boot basics"
                }
              ]
            }
            """);
        var validator = new AiPlanningDraftValidator();

        var result = validator.Validate(draft);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_rejects_unknown_action_type()
    {
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_project",
              "summary": "Bad plan.",
              "actions": [
                { "type": "delete_item", "public_ref": "t-0426-1" }
              ]
            }
            """);
        var validator = new AiPlanningDraftValidator();

        var result = validator.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("Unsupported action type", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_rejects_new_weekly_task_with_non_default_tag()
    {
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_weekly",
              "summary": "Weekly plan.",
              "actions": [
                {
                  "type": "create_task",
                  "tag": "academia",
                  "collection": "week",
                  "content": "Go to the gym"
                }
              ]
            }
            """);
        var validator = new AiPlanningDraftValidator();

        var result = validator.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("new_weekly", StringComparison.Ordinal));
        Assert.Contains(result.Errors, error => error.Contains("default", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_rejects_new_project_without_non_default_tag()
    {
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_project",
              "summary": "Project plan.",
              "actions": [
                {
                  "type": "create_task",
                  "tag": "default",
                  "collection": "today",
                  "content": "Define scope"
                }
              ]
            }
            """);
        var validator = new AiPlanningDraftValidator();

        var result = validator.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("non-default tag", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_rejects_revise_project_move_without_public_ref()
    {
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "revise_project",
              "summary": "Review project.",
              "actions": [
                {
                  "type": "move_task",
                  "collection": "week"
                }
              ]
            }
            """);
        var validator = new AiPlanningDraftValidator();

        var result = validator.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("public_ref", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_accepts_java_roadmap_scenario_distribution()
    {
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_project",
              "summary": "Java study roadmap.",
              "actions": [
                { "type": "create_tag", "name": "estudo-java" },
                { "type": "create_note", "tag": "estudo-java", "content": "Java roadmap scope" },
                { "type": "create_task", "tag": "estudo-java", "collection": "today", "content": "Install JDK" },
                { "type": "create_task", "tag": "estudo-java", "collection": "week", "content": "Study syntax" },
                { "type": "create_task", "tag": "estudo-java", "collection": "week", "content": "Practice OOP" },
                { "type": "create_task", "tag": "estudo-java", "collection": "week", "content": "Read collections" },
                { "type": "create_task", "tag": "estudo-java", "collection": "week", "content": "Build CLI exercise" },
                { "type": "create_task", "tag": "estudo-java", "collection": "month", "content": "Study streams" },
                { "type": "create_task", "tag": "estudo-java", "collection": "backlog", "content": "Learn Spring Boot" }
              ]
            }
            """);
        var validator = new AiPlanningDraftValidator();

        var result = validator.Validate(draft);

        Assert.True(result.IsValid);
        Assert.Equal(1, draft.Actions.Count(action => action.Collection == "today"));
        Assert.Equal(4, draft.Actions.Count(action => action.Collection == "week"));
        Assert.Contains(draft.Actions, action => action.Collection == "month");
        Assert.Contains(draft.Actions, action => action.Collection == "backlog");
    }

    [Fact]
    public void Validate_accepts_nutrition_chatbot_project_scenario()
    {
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_project",
              "summary": "Nutrition chatbot first version.",
              "actions": [
                { "type": "create_tag", "name": "chatbot-nutrition" },
                { "type": "create_note", "tag": "chatbot-nutrition", "content": "Nutrition chatbot scope" },
                { "type": "create_task", "tag": "chatbot-nutrition", "collection": "today", "content": "Define supported calculations" },
                { "type": "create_task", "tag": "chatbot-nutrition", "collection": "week", "content": "Design conversation flow" },
                { "type": "create_task", "tag": "chatbot-nutrition", "collection": "month", "content": "Implement calculator service" }
              ]
            }
            """);
        var validator = new AiPlanningDraftValidator();

        var result = validator.Validate(draft);

        Assert.True(result.IsValid);
        Assert.All(
            draft.Actions.Where(action => action.Type is "create_task" or "create_note"),
            action => Assert.Equal("chatbot-nutrition", action.Tag));
    }

    [Fact]
    public void Validate_accepts_gym_habit_project_tag_scenario()
    {
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_project",
              "summary": "Gym habit tracking.",
              "actions": [
                { "type": "create_tag", "name": "gym" },
                { "type": "create_note", "tag": "gym", "content": "Goal: go to the gym three times per week." },
                { "type": "create_task", "tag": "gym", "collection": "week", "content": "Gym session 1" },
                { "type": "create_task", "tag": "gym", "collection": "week", "content": "Gym session 2" },
                { "type": "create_task", "tag": "gym", "collection": "week", "content": "Gym session 3" }
              ]
            }
            """);
        var validator = new AiPlanningDraftValidator();

        var result = validator.Validate(draft);

        Assert.True(result.IsValid);
        Assert.Equal(3, draft.Actions.Count(action => action.Collection == "week"));
    }

    [Fact]
    public void Validate_rejects_new_project_with_conflicting_project_tags()
    {
        var draft = AiPlanningDraftParser.Parse(
            """
            {
              "mode": "new_project",
              "summary": "Conflicting project.",
              "actions": [
                { "type": "create_tag", "name": "project-a" },
                { "type": "create_task", "tag": "project-b", "collection": "today", "content": "Define scope" }
              ]
            }
            """);
        var validator = new AiPlanningDraftValidator();

        var result = validator.Validate(draft);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("one non-default tag", StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_rejects_malformed_json()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => AiPlanningDraftParser.Parse("{"));

        Assert.Contains("malformed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
