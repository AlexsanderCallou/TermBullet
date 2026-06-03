using TermBullet.Application.Ai;

namespace TermBullet.Tests.Application.Ai;

public sealed class AiPlanningResponseParserTests
{
    [Fact]
    public void ParseFlexible_returns_chat_message_from_response_envelope()
    {
        var (draft, message) = AiPlanningResponseParser.ParseFlexible(
            """
            {
              "kind": "chat",
              "message": "I can help shape the Rust study roadmap first.",
              "draft_ready": false,
              "draft": null
            }
            """);

        Assert.Null(draft);
        Assert.Equal("I can help shape the Rust study roadmap first.", message);
    }

    [Fact]
    public void ParseFlexible_returns_draft_from_response_envelope()
    {
        var (draft, message) = AiPlanningResponseParser.ParseFlexible(
            """
            {
              "kind": "draft",
              "message": "Draft ready for approval.",
              "draft_ready": true,
              "draft": {
                "mode": "new_project",
                "summary": "Rust study roadmap.",
                "actions": [
                  {
                    "type": "create_tag",
                    "name": "estudos-rust"
                  },
                  {
                    "type": "create_task",
                    "tag": "estudos-rust",
                    "collection": "today",
                    "content": "Start Rust ownership study"
                  }
                ]
              }
            }
            """);

        Assert.NotNull(draft);
        Assert.Null(message);
        Assert.Equal("new_project", draft.Mode);
        Assert.Equal(2, draft.Actions.Count);
    }

    [Fact]
    public void ParseFlexible_extracts_draft_from_markdown_fenced_json()
    {
        var (draft, message) = AiPlanningResponseParser.ParseFlexible(
            """
            Here is the proposed plan:

            ```json
            {
              "mode": "new_project",
              "summary": "Build the planning flow.",
              "actions": [
                {
                  "type": "create_tag",
                  "name": "planning"
                },
                {
                  "type": "create_task",
                  "tag": "planning",
                  "collection": "week",
                  "content": "Fix draft detection"
                }
              ]
            }
            ```
            """);

        Assert.NotNull(draft);
        Assert.Null(message);
        Assert.Equal("new_project", draft.Mode);
        Assert.Equal(2, draft.Actions.Count);
    }

    [Fact]
    public void ParseFlexible_ignores_non_draft_json_before_real_draft()
    {
        var (draft, message) = AiPlanningResponseParser.ParseFlexible(
            """
            metadata: {}
            {
              "mode": "new_weekly",
              "summary": "Weekly Java study plan.",
              "actions": [
                {
                  "type": "create_task",
                  "tag": "default",
                  "collection": "week",
                  "content": "Practice Java loops"
                }
              ]
            }
            """);

        Assert.NotNull(draft);
        Assert.Null(message);
        Assert.Equal("Weekly Java study plan.", draft.Summary);
    }

    [Fact]
    public void ParseFlexible_hides_unparseable_json_from_user_message()
    {
        var (draft, message) = AiPlanningResponseParser.ParseFlexible(
            """
            ```json
            { "mode": "new_project", "summary": "Broken", "actions": [
            ```
            """);

        Assert.Null(draft);
        Assert.Equal("I received a draft-shaped response, but it was not valid TermBullet draft JSON. Ask for a revised plan.", message);
    }
}
