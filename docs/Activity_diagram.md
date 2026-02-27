‘‘‘mermaid
stateDiagram-v2
[*] --> ReceiveRequest
ReceiveRequest --> CheckRole
state check_role <<choice>>
CheckRole --> check_role
check_role --> RejectCommand : Role is Viewer
check_role --> SendToRobot : Role is Commander
RejectCommand --> [*]
SendToRobot --> [*]
SendToRobot --> check_api
check_api --> LogSuccess : API is responsive
check_api --> LogError : API is unresponsive
‘‘‘