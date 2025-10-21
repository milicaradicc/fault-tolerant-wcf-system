# Secure Messaging Client/Server Documentation

## Overview

This project implements a secure messaging system using WCF (Windows Communication Foundation) with duplex communication and Entity Framework for data persistence. It consists of a WCF service that manages multiple clients, facilitates encrypted message exchange, and maintains comprehensive logging of all system activities.

## Architecture

### System Components

1. **WCF Service (Server)** - Manages client connections, handles message routing, monitors client health, and persists data
2. **Console Client Application** - Connects to the service and enables users to send/receive encrypted messages
3. **Duplex Communication** - Bidirectional communication using `wsDualHttpBinding`
4. **AES Encryption** - End-to-end message encryption using AES-128
5. **Entity Framework** - Database persistence layer using Code First approach
6. **SQL Server LocalDB** - Local database for storing client data and logs

## Project Structure

### Service Project

```
Service/
├── IService1.cs              # Service contract definitions
├── Service1.svc.cs           # Service implementation
├── ServiceDbContext.cs       # EF DbContext configuration
├── Clients/
│   ├── ClientData.cs         # Client entity model
│   ├── ClientStatus.cs       # Client status enumeration
│   └── ClientRepository.cs   # Client data access layer
├── Logs/
│   ├── Log.cs                # Log entity model
│   ├── LogType.cs            # Log type enumeration
│   └── LogRepository.cs      # Log data access layer
├── Crypto.cs                 # Encryption utilities
├── Web.config                # Service configuration
└── packages.config           # NuGet packages (Entity Framework 6.5.1)
```

### Client Project

```
Client/
├── Program.cs                # Entry point and command interface
├── Services/
│   ├── Client.cs             # Client service logic
│   ├── Callback.cs           # Callback handler
│   └── Crypto.cs             # Encryption utilities
└── App.config                # Client configuration
```

## Core Components

### 1. Service Contract (IService1.cs)

Defines the communication contract between client and server:

**Service Operations:**
- `RegisterClient()` - Registers a new client and returns a unique GUID
- `SendHeartbeat(Guid clientId)` - Updates client's last activity timestamp
- `SetStatus(Guid clientId, ClientStatus status)` - Updates client status (not implemented)
- `SendMessage(Guid fromClientId, Guid toClientId, string encryptedMessage)` - Routes encrypted messages

**Callback Operations:**
- `OnStart()` - Notifies client to transition from Standby to Running state
- `OnMessageReceived(Guid fromClientId, string encryptedMessage)` - Delivers received messages

### 2. Service Implementation (Service1.svc.cs)

**Key Features:**

- **Singleton Service**: Uses `InstanceContextMode.Single` with `ConcurrencyMode.Multiple` for thread-safe shared state
- **Thread-Safe Callback Management**: Uses `ConcurrentDictionary<Guid, ICallback>` for managing callback channels
- **Database Persistence**: All client data and logs are persisted using Entity Framework
- **Health Monitoring**: Background timer checks client heartbeats every 5 seconds
- **Load Balancing**: Automatically starts up to 2 running clients, promoting standby clients when slots become available
- **Comprehensive Logging**: All operations are logged with specific log types

**Client States:**
- `Standby` - Registered but not actively processing
- `Running` - Active and processing (max 2 concurrent)
- `Dead` - Failed to send heartbeat within 30 seconds

**Configuration Constants:**
```csharp
CheckInterval = 5000ms               // Health check frequency
ClientInactivityThreshold = 30000ms  // Heartbeat timeout
MaxRunningClients = 2                // Concurrent active clients
```

**Thread Safety:**
- Uses `ConcurrentDictionary` for callback storage
- Implements `StartLock` object for synchronizing client promotion logic

### 3. Database Layer (Entity Framework)

**ServiceDbContext:**
- Inherits from `DbContext`
- Manages two DbSets: `Clients` and `Logs`
- Uses Code First approach with `CreateDatabaseIfNotExists` initializer
- Resets client data on service startup via `ResetDatabase()` method
- Configures optional relationship between Log and ClientData

**Connection String:**
```
Data Source=(LocalDb)\MSSQLLocalDB;
Initial Catalog=ServiceDb;
Integrated Security=True;
```

### 4. Repository Pattern

**ClientRepository (Data Access Layer):**
- `AddClient(ClientData client)` - Adds new client to database
- `GetClient(Guid clientId)` - Retrieves client by ID
- `UpdateClient(Guid clientId, ClientData updatedClient)` - Updates client data
- `GetClientsWithStatus(ClientStatus status)` - Retrieves clients by status
- `GetAllInactiveClients(int clientInactivityTreshold)` - Returns clients exceeding inactivity threshold

**LogRepository (Data Access Layer):**
- `AddLog(Log log)` - Adds new log entry to database

### 5. Logging System

**Log Entity:**
```csharp
public class Log
{
    public int Id { get; set; }
    public Guid? ClientId { get; set; }
    public LogType Type { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public ClientData ClientData { get; set; }  // Navigation property
}
```

**LogType Enumeration:**
- `ClientRegistered` - Client successfully registered
- `ClientStarted` - Client promoted to Running state
- `ClientDied` - Client marked as Dead due to heartbeat timeout
- `HeartbeatReceived` - Heartbeat received from client
- `SentMessage` - Message successfully routed
- `Error` - Error occurred (e.g., unknown client)

### 6. Client Application (Client.cs)

**Responsibilities:**

- **Registration**: Connects to service and obtains unique client ID
- **Heartbeat**: Sends periodic heartbeat every 10 seconds
- **Status Monitoring**: Displays status updates every 5 seconds when running
- **Message Handling**: Encrypts outgoing messages and decrypts incoming messages

**Timers:**
- Heartbeat timer: 10-second interval
- Status timer: 5-second interval (displays "Working..." when status is Running)

### 7. Encryption (Crypto.cs)

**Algorithm**: AES (Advanced Encryption Standard)

**Configuration:**
- Key: `"1234567890123456"` (128-bit)
- IV: `"6543210987654321"` (128-bit)
- Mode: CBC (Cipher Block Chaining)
- Output: Base64-encoded ciphertext

**Methods:**
- `Encrypt(string plainText)` - Encrypts plaintext to Base64 string
- `Decrypt(string cipherText)` - Decrypts Base64 string to plaintext

⚠️ **Security Note**: Hardcoded keys are used for demonstration purposes. Production systems should use secure key management (e.g., Azure Key Vault, HSM).

## Communication Flow

### 1. Client Registration

```
Client                          Service                          Database
  |                                |                                |
  |------ RegisterClient() ------->|                                |
  |                                |--- Insert ClientData --------->|
  |                                |--- Insert Log (Registered) --->|
  |                                | Generate GUID                  |
  |                                | Store callback channel         |
  |                                | Call StartClients()            |
  |<------ Return clientId --------|                                |
  |                                |                                |
  |<------ OnStart() callback -----|  (if promoted to Running)      |
  |                                |--- Update ClientData --------->|
  |                                |--- Insert Log (Started) ------>|
```

### 2. Message Exchange

```
Sender                          Service                          Receiver
  |                                |                                |
  |-- SendMessage(to, encrypted)-->|                                |
  |                                |--- Insert Log (SentMessage) -->| DB
  |                                |-- OnMessageReceived(from, enc)->|
  |                                |                                | Decrypt
  |                                |                                | Display
```

### 3. Health Monitoring

```
Client                          Service                          Database
  |                                |                                |
  |--- SendHeartbeat(clientId) --->|                                |
  |                                |--- Update LastHeartbeat ------>|
  |                                |--- Insert Log (Heartbeat) ---->|
  |                                |                                |
  |  (every 10 seconds)            | (Check every 5 seconds)        |
  |                                |                                |
  |                                | If no heartbeat > 30s:         |
  |                                |   - Update Status to Dead ---->|
  |                                |   - Insert Log (ClientDied) -->|
  |                                |   - Start standby clients      |
```

## Configuration

### Service Configuration (Web.config)

**WCF Service Configuration:**
```xml
<service name="Service.Service1" behaviorConfiguration="ServiceBehavior">
  <endpoint address=""
    binding="wsDualHttpBinding"
    contract="Service.IService1" />
  <endpoint address="mex"
    binding="mexHttpBinding"
    contract="IMetadataExchange" />
</service>
```

**Entity Framework Configuration:**
```xml
<entityFramework>
  <providers>
    <provider invariantName="System.Data.SqlClient"
              type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
  </providers>
</entityFramework>

<connectionStrings>
  <add name="ServiceDbContext"
       connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ServiceDb;Integrated Security=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Bindings**: `wsDualHttpBinding` enables duplex communication over HTTP

**Endpoint**: `http://localhost:56420/Service1.svc`

### Client Configuration (App.config)

```xml
<endpoint address="http://localhost:56420/Service1.svc"
  binding="wsDualHttpBinding"
  bindingConfiguration="WSDualHttpBinding_IService1"
  contract="ServiceReference.IService1"
  name="WSDualHttpBinding_IService1" />
```

## Data Models

### ClientData Entity

```csharp
public class ClientData
{
    [Key]
    public Guid Id { get; set; }
    public ClientStatus Status { get; set; }
    public DateTime LastHeartbeat { get; set; }
}
```

### ClientStatus Enumeration

```csharp
[DataContract]
public enum ClientStatus
{
    [EnumMember]
    Running,   // Actively processing
    
    [EnumMember]
    Standby,   // Registered but waiting
    
    [EnumMember]
    Dead       // Failed heartbeat check
}
```

### Log Entity

```csharp
public class Log
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("ClientData")]
    public Guid? ClientId { get; set; }
    public virtual ClientData ClientData { get; set; }

    [Required]
    public LogType Type { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
    [StringLength(500)]
    public string Message { get; set; }
}
```

### LogType Enumeration

```csharp
[DataContract]
public enum LogType
{
    [EnumMember]
    ClientRegistered,
    
    [EnumMember]
    ClientStarted,
    
    [EnumMember]
    ClientDied,
    
    [EnumMember]
    HeartbeatReceived,
    
    [EnumMember]
    SentMessage,
    
    [EnumMember]
    Error
}
```

## User Interface

### Client Commands

| Command | Syntax | Description |
|---------|--------|-------------|
| `msg` | `msg <recipient-id> <message>` | Send encrypted message to recipient |
| `info` | `info` | Display client ID, status, and timestamp |
| `clear` | `clear` | Clear console screen |
| `commands` | `commands` | Display help menu |
| `exit` | `exit` | Disconnect and terminate client |

### Example Usage

```
>> msg 12345678-1234-1234-1234-123456789abc Hello World!
[✓] Message sent to 12345678-1234-1234-1234-123456789abc

>> info
┌─────────────────────────────────────────┐
│          CLIENT INFORMATION             │
├─────────────────────────────────────────┤
│  ID:     12345678-1234-1234-1234-123456789abc  │
│  Status: Running                        │
│  Time:   2025-10-20 14:30:45           │
└─────────────────────────────────────────┘
```

## Technical Details

### Threading & Concurrency

- **Service**: Singleton instance with multiple concurrent threads
- **Thread Safety**: 
  - `ConcurrentDictionary<Guid, ICallback>` for thread-safe callback management
  - `lock (StartLock)` for synchronizing client promotion logic
  - Database operations are thread-safe through EF DbContext per-operation instantiation
- **Timers**: `System.Timers.Timer` for periodic health checks

## Dependencies

### NuGet Packages

**Service Project:**
- Entity Framework 6.5.1
- System.ServiceModel (WCF)
- System.Runtime.Serialization
- System.Data.Entity

**Client Project:**
- System.ServiceModel (WCF)
- System.Runtime.Serialization

## Deployment

### Prerequisites

- .NET Framework 4.7.2
- SQL Server LocalDB (included with Visual Studio)
- IIS or IIS Express (for hosting WCF service)
- Visual Studio 2017+ (for development)
- Entity Framework 6.5.1 (installed via NuGet)

### Running the Service

1. Restore NuGet packages (Entity Framework)
2. Build the Service project
3. Host in IIS or run via Visual Studio (F5)
4. Verify service is accessible at `http://localhost:56420/Service1.svc`
5. Database will be created automatically on first request

### Running the Client

1. Ensure service is running
2. Build and run Client.exe
3. Client automatically registers upon startup
4. Use commands to interact with the system