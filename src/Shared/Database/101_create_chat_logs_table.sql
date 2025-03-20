CREATE TABLE chat_logs (
    session_id VARCHAR NOT NULL,
    message_id VARCHAR DEFAULT gen_random_uuid(),
    question TEXT,
    request_timestamp TIMESTAMP NOT NULL,
    request_year INTEGER GENERATED ALWAYS AS (EXTRACT(YEAR FROM request_timestamp)) STORED,
    request_month INTEGER GENERATED ALWAYS AS (EXTRACT(MONTH FROM request_timestamp)) STORED,
    answer TEXT,
    confidence_score DECIMAL,
    source VARCHAR,
    PRIMARY KEY (session_id, message_id)
);

CREATE INDEX idx_session_id ON chat_logs (session_id);  -- This is a non-unique index
CREATE INDEX idx_request_year ON chat_logs (request_year);  -- This is a non-unique index
CREATE INDEX idx_request_month ON chat_logs (request_month);  -- This is a non-unique index