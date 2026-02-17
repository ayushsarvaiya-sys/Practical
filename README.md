# Multi-Client Chunk-Based File Upload System

This project implements a distributed system for concurrent file uploads using a line-based TCP protocol, with real-time progress monitoring via WebSockets and an Angular-based dashboard.

## Project Structure

- **Client**: A C# Console Application representing a TCP client that chunks files and sends them to the server.
- **Prac (Server)**: An ASP.NET Core Web API that manages TCP connections, processes file chunks, and broadcasts progress updates via WebSockets.
- **Frontend**: An Angular application that provides a real-time visualization of upload progress across all clients.

## Key Features

- **Chunk-Based Upload**: Files are split into small chunks (10KB by default) and encoded in Base64 for reliable transmission.
- **Multi-Client Support**: The server handles multiple concurrent TCP connections independently using unique `fileId` tracking.
- **Real-Time Monitoring**: WebSockets are used to broadcast progress updates (`Uploading`, `Completed`) to all connected frontend clients.
- **Line-Based Protocol**: Simple text-based protocol for easy debugging and low overhead.

## Architecture & Protocol

### 1. Communication Protocol
The system uses a pipe-delimited, line-based protocol over TCP. Each command occupies exactly one line.

- **START**: `START|fileId|fileName|totalChunks|totalFileSize`
- **CHUNK**: `CHUNK|fileId|chunkNumber|base64Data`
- **END**: `END|fileId`

### 2. Progress Calculation
Progress is calculated dynamically as:
$$\text{progress} = \frac{\text{receivedChunks}}{\text{totalChunks}} \times 100$$

### 3. WebSocket Broadcast
The server sends JSON updates to connected web clients for every chunk processed:
```json
{
  "fileId": "uuid",
  "fileName": "report.pdf",
  "progress": 40,
  "status": "Uploading"
}
```

## Technologies Used

- **Backend**: C# / .NET 10.0, ASP.NET Core, TCP Sockets, WebSockets, Entity Framework Core (SQL Server).
- **Frontend**: Angular 19+ (based on `package.json`), TypeScript, RxJS.
- **Database**: SQL Server (for tracking state/history if implemented).

## How to Run

### 1. Prerequisites
- [.NET SDK 10+](https://dotnet.microsoft.com/download)
- [Node.js & npm](https://nodejs.org/)
- SQL Server (Update connection string in `Prac/appsettings.json`)

### 2. Start the Server
Navigate to the `Prac` directory and run:
```bash
dotnet run
```
The server will start the Web API on standard ports (e.g., 5000/5001) and the TCP listener on port **7000**.

### 3. Start the Frontend
Navigate to the `Frontend` directory:
```bash
npm install
npm start
```
Open [http://localhost:4200](http://localhost:4200) in your browser.

### 4. Run the Client
Navigate to the `Client` directory and run:
```bash
dotnet run
```
Enter the absolute path of a file to begin the upload. Multiple clients can be run simultaneously.

## Non-Functional Requirements
- **Storage**: Completed files are saved in the `Prac/uploads/` directory.
- **Resilience**: Handles invalid Base64 data, missing protocol steps, and unexpected disconnections.
- **Scalability**: Asynchronous processing allows the server to remain responsive during heavy upload loads.
