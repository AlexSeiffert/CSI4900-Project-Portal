# Orchestrations

Reserved for workflows that require multiple necessary CRUD operations across services.
There are currently no concrete orchestrations or orchestration interfaces.

Controllers call services directly when an operation needs only one service call.
Do not introduce orchestrations for validation or existence checks, including reads
whose results are discarded solely to check whether a resource exists.

For future workflows, place implementations here and their contracts in `Interfaces/`,
register them in `Program.cs`, and add workflow tests. `OrchestrationException` and
its central exception-handler support remain available for those workflows.
