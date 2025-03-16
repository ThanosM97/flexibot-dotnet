# Project Structure

This document outlines the directory structure and organization of the project, detailing the purpose and content of each component to facilitate easier navigation and understanding of the project's architecture.

## src
The `src/` directory contains all the main components of the application, including API, shared utilities, models, services, and worker processes, providing a comprehensive infrastructure for handling various functionalities such as document processing, event-driven operations, and real-time communications.

### Api
- **Api.csproj**: Project file for building the API project.
- **appsettings.json**: Configuration settings for the API.
- **Controllers**: Handles HTTP requests.
  - **ChatController.cs**: Manages chat-related actions and events.
  - **DocumentController.cs**: Manages RAG document-related actions (upload, status, download, delete).
  - **QnAController.cs**: Manages QnA-related actions (upload, delete, download).
- **Hubs**: SignalR Hubs for real-time communication.
  - **ChatHub.cs**: Hub for managing chat sessions and streaming responses.
- **Program.cs**: Main entry point for the API project.
- **Services**: Implementation of service functionalities.
  - **ChatResponseStreamConsumer.cs**: Service for handling streamed chat responses.

### Shared
- **Database**: SQL scripts and database-related services.
  - **001_create_documents_table.sql**: Script for creating the documents table.
- **Events**: Event definitions for the event-driven architecture.
  - **ChatPromptedEvent.cs**
  - **ChatResponseStreamedEvent.cs**
  - **DocumentChunkedEvent.cs**
  - **DocumentDeletedEvent.cs**
  - **DocumentEmbeddedEvent.cs**
  - **DocumentIndexedEvent.cs**
  - **DocumentParsedEvent.cs**
  - **DocumentStatusEvent.cs**
  - **DocumentUploadedEvent.cs**
  - **QnADeletedEvent.cs**
  - **QnAUploadedEvent.cs**
- **Factories**: Factories for creating service instances.
  - **AI**
    - **Language**
      - **ChatFactory.cs**
      - **EmbeddingFactory.cs**
      - **QnAFactory.cs**
    - **RAG**
      - **RAGFactory.cs**
  - **Search**
    - **VectorDatabaseFactory.cs**
- **Interfaces**: Interfaces define contracts for the services.
  - **AI**
    - **Language**
      - **IChatService.cs**
      - **IEmbeddingService.cs**
      - **IQnAService.cs**
      - **ITextNormalizationService.cs**
    - **RAG**
      - **IRagService.cs**
  - **Database**
    - **IDocumentRepositoryService.cs**
  - **Search**
    - **IVectorDatabaseService.cs**
  - **Storage**
    - **IStorageService.cs**
- **Models**: Models represent the data structures used across services.
  - **ChatBotResult.cs**
  - **ChatCompletionMessage.cs**
  - **ChatCompletionResult.cs**
  - **ChatRequest.cs**
  - **DocumentChunk.cs**
  - **DocumentMetadata.cs**
  - **QnARecord.cs**
  - **QnAResult.cs**
  - **QnASearchResult.cs**
  - **RAGResult.cs**
  - **SearchResult.cs**
- **Prompts**: Templates for prompts used in RAG services.
  - **RAGPrompts.cs**
- **Services**: Implementation of service functionalities.
  - **AI**
    - **Language**
      - **OllamaChatService.cs**
      - **OllamaEmbeddingService.cs**
      - **OllamaService.cs**: Base service interaction with Ollama models.
      - **SemanticQnAService.cs**
      - **TextNormalizationService.cs**
    - **RAG**
      - **BaseRAGService.cs**: Base class for RAG-related service logic.
      - **HyDEService.cs**: Implementation of HyDE (hypothetical document extraction) algorithm.
      - **SimpleRAGService.cs**: Implementation of a simple RAG algorithm.
  - **Database**
    - **PostgresService.cs**
  - **RabbitMQConsumerBase.cs**: Base class for RabbitMQ consumers, handling common logic.
  - **RabbitMQPublisher.cs**: Service for publishing events to RabbitMQ.
  - **Search**
    - **VectorDatabase**
      - **QdrantService.cs**
  - **Storage**
    - **MinioService.cs**
- **Shared.csproj**: Project file for building the shared components project.
- **Utils**: Utilities providing helper functions and logic.
  - **RAGHelpers.cs**

### Workers
- **ChunkerWorker**: Worker handling RAG document chunking.
  - **appsettings.json**: Configuration settings for the Chunker Worker.
  - **ChunkerWorker.csproj**: Project file for the Chunker Worker.
  - **Program.cs**: Main entry point for the Chunker Worker.
  - **Properties**
    - **launchSettings.json**: Launch settings for the development environment.
  - **Services**
    - **DocumentChunker.cs**: Service logic for splitting documents into chunks.
    - **RabbitMQConsumer.cs**: RabbitMQ consumer specific to document chunking.
  - **Worker.cs**: Main worker logic.

- **DeleterWorker**: Worker handling RAG document deletions.
  - **appsettings.json**: Configuration settings for the Deleter Worker.
  - **DeleterWorker.csproj**: Project file for the Deleter Worker.
  - **Program.cs**: Main entry point for the Deleter Worker.
  - **Properties**
    - **launchSettings.json**: Launch settings for the development environment.
  - **Services**
    - **DocumentDeleter.cs**: Logic for deleting documents.
    - **RabbitMQConsumer.cs**: RabbitMQ consumer specific to deletions.
  - **Worker.cs**: Main worker logic.

- **EmbedderWorker**: Worker responsible for generating RAG document embeddings.
  - **appsettings.json**: Configuration settings for the Embedder Worker.
  - **EmbedderWorker.csproj**: Project file for the Embedder Worker.
  - **Program.cs**: Main entry point for the Embedder Worker.
  - **Properties**
    - **launchSettings.json**: Launch settings for the development environment.
  - **Services**
    - **DocumentEmbedder.cs**: Service for generating embeddings for documents.
    - **RabbitMQConsumer.cs**: RabbitMQ consumer specific to embedding generation.
  - **Worker.cs**: Main worker logic.

- **IndexerWorker**: Worker for indexing RAG documents into the vector database.
  - **appsettings.json**: Configuration settings for the Indexer Worker.
  - **IndexerWorker.csproj**: Project file for the Indexer Worker.
  - **Program.cs**: Main entry point for the Indexer Worker.
  - **Properties**
    - **launchSettings.json**: Launch settings for the development environment.
  - **Services**
    - **DocumentIndexer.cs**: Service logic for indexing document data in the vector database.
    - **RabbitMQConsumer.cs**: RabbitMQ consumer specific to indexing operations.
  - **Worker.cs**: Main worker logic.

- **ParserWorker**: Worker for parsing RAG documents into text.
  - **appsettings.json**: Configuration settings for the Parser Worker.
  - **ParserWorker.csproj**: Project file for the Parser Worker.
  - **Program.cs**: Main entry point for the Parser Worker.
  - **Properties**
    - **launchSettings.json**: Launch settings for the development environment.
  - **Services**
    - **DocumentParser.cs**: Service for extracting text from documents.
    - **RabbitMQConsumer.cs**: RabbitMQ consumer specific to document parsing.
  - **Worker.cs**: Main worker logic.

- **QnAWorker**: Worker for handling QnA-related tasks.
  - **appsettings.json**: Configuration settings for the QnA Worker.
  - **Program.cs**: Main entry point for the QnA Worker.
  - **Properties**
    - **launchSettings.json**: Launch settings for the development environment.
  - **QnAWorker.csproj**: Project file for the QnA Worker.
  - **Services**
    - **QnAProcessor.cs**: Service for processing QnA-related actions.
    - **RabbitMQConsumerDelete.cs**: Consumer for handling QnA deletion requests.
    - **RabbitMQConsumerUpload.cs**: Consumer for handling QnA upload requests.
  - **Worker.cs**: Main worker logic.

- **ResponseWorker**: Worker for generating responses to chat queries.
  - **appsettings.json**: Configuration settings for the Response Worker.
  - **Program.cs**: Main entry point for the Response Worker.
  - **Properties**
    - **launchSettings.json**: Launch settings for the development environment.
  - **ResponseWorker.csproj**: Project file for the Response Worker.
  - **Services**
    - **ChatBot.cs**: Service for processing chat requests and generating responses.
    - **RabbitMQConsumer.cs**: RabbitMQ consumer for handling requests and streaming responses.
  - **Worker.cs**: Main worker logic.

- **StatusWorker**: Worker for updating document processing status.
  - **appsettings.json**: Configuration settings for the Status Worker.
  - **Program.cs**: Main entry point for the Status Worker.
  - **Properties**
    - **launchSettings.json**: Launch settings for the development environment.
  - **Services**
    - **RabbitMQConsumer.cs**: RabbitMQ consumer for processing status update events.
    - **StatusUpdater.cs**: Logic for updating document statuses based on processing events.
  - **StatusWorker.csproj**: Project file for the Status Worker.
  - **Worker.cs**: Main worker logic.