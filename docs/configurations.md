# Available Configurations

This document outlines the environment variables used in the Docker setup and their default values. You can modify these variables to suit your specific requirements.

| Category                | Key                            | Default Value                                                                    | Alternatives                  |
|-------------------------|--------------------------------|----------------------------------------------------------------------------------|-------------------------------|
| **General**             | `ASPNETCORE_ENVIRONMENT`       | Development                                                                      |                               |
| **Database**            | `ConnectionStrings__Postgres`  | `Host=postgres;Port=5432;Database=flexibot;Username=postgres;Password=postgres`  |                               |
| **Cache**               | `ConnectionStrings__REDIS`     | `redis:6379,abortConnect=false`                                                  |                               |
| **Search**              | `SEARCH__PROVIDER`             | Qdrant                                                                           |                               |
|                         | `SEARCH__DOCUMENT_COLLECTION`  | documents                                                                        |                               |
|                         | `SEARCH__QNA_COLLECTION`       | qna                                                                              |                               |
|                         | `SEARCH__VECTOR_SIZE`          | 384                                                                              |                               |
| **Qdrant**              | `QDRANT__HOST`                 | qdrant                                                                           |                               |
|                         | `QDRANT__PORT`                 | 6334                                                                             |                               |
| **Message Broker**      | `RABBITMQ__HOST`               | rabbitmq                                                                         |                               |
| **Storage**             | `MINIO__ENDPOINT`              | minio:9000                                                                       |                               |
|                         | `MINIO__ACCESS_KEY`            | minioadmin                                                                       |                               |
|                         | `MINIO__SECRET_KEY`            | minioadmin                                                                       |                               |
|                         | `MINIO__DOCUMENT_BUCKET`       | documents                                                                        |                               |
|                         | `MINIO__QNA_BUCKET`            | qna                                                                              |                               |
| **Embedding and Chat**  | `EMBEDDING_PROVIDER`           | Ollama                                                                           |                               |
|                         | `CHAT_PROVIDER`                | Ollama                                                                           |                               |
|                         | `OLLAMA__ENDPOINT`             | `http://ollama:11434`                                                            |                               |
|                         | `OLLAMA__EMBEDDING_MODEL`      | all-minilm                                                                       | `Any ollama embedding model`  |
|                         | `OLLAMA__LLM_MODEL`            | llama3.2:1b                                                                      | `Any ollama LLM model`        |
| **RAG**                 | `RAG__METHOD`                  | simple                                                                           | `simple` or `hyde`            |
|                         | `RAG__TOP_K`                   | 5                                                                                |                               |
|                         | `RAG__CONFIDENCE_THRESHOLD`    | 0.7                                                                              |                               |
|                         | `RAG__DEFAULT_ANSWER`          | I don't know the answer to this question.                                        |                               |
| **QNA**                 | `QNA__METHOD`                  | semantic                                                                         |                               |
|                         | `QNA__CONFIDENCE_THRESHOLD`    | 0.7                                                                              |                               |
|                         | `QNA__DEFAULT_ANSWER`          | I don't know the answer to this question.                                        |                               |
| **Redis**               | `REDIS__TTL`                   | 00:30:00                                                                         |                               |

---
