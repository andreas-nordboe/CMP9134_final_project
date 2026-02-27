‘‘‘mermaid
flowchart LR
%% Define Actors
C[Commander]
V[Viewer]
A[Auditor]

%% Define Use Cases
Move((Move Robot))
Status((View Status))
ResetRobot((Reset Robot))
ViewLogs((View Logs))

%% Connect Actors to Use Cases
C --> Move
C --> Status
V --> Status
C --> ResetRobot
A --> ViewLogs
‘‘‘