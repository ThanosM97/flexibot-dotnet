CREATE TABLE documents (
    id TEXT PRIMARY KEY DEFAULT gen_random_uuid(),
    object_storage_key TEXT NOT NULL,
    file_name TEXT NOT NULL,
    content_type TEXT,
    language TEXT,
    extension TEXT,
    size DOUBLE PRECISION NOT NULL,
    uploaded_at TIMESTAMP NOT NULL,
    tags TEXT,
    status INTEGER DEFAULT 0,
);
