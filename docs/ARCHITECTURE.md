The system architecture follows the MVC (Model, View, Controller) architectural pattern.


## Modelling
The chosen .NET Blazor frontend acts as the view, rendering the UI in the browser to the end-user so that they can see and interact with the robot. The ASP.NET Core backend is the controller that processes HTTP requests. It serves a HTTP REST API. Furthermore, it runs logic based on requests received by the frontend and communicates with the external robot REST API. 

The model layer is implemented using a shared class library that holds entities and data transfer objects (DTOs) that are used by both the frontend and the backend. In addition, Entity Framework Core DbContext provides data access functionality. The SQLite database persists the application data, rather than acting as the model itself.


# Facade Services
The system uses singleton throughout to reuse and avoid duplicating code. ASP.NET Core achieves this through use of Services that are registered in Program.cs as either Scoped or Singleton services. For example, the robot move command function exists only once but can be reused anywhere on the backend. Many services are scoped instead of singleton, meaning they run once per HTTP request and are injected through the use of dependency injection. This increases flexibility as the dependencies are provided from the outside so they can be more easily swapped, which in turn improves testing capabilities.

Access to these abstract services are isolated within the frontend and backend as they both serve different logic (i.e., the robot client on the frontend sends commands to the backend and the robot client on the backend sends commands to the external backend API).   