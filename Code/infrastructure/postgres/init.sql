-- Creates the staging database alongside the prod database.
-- Runs once on first container start (docker-entrypoint-initdb.d).
CREATE DATABASE doko_analog_staging;
GRANT ALL PRIVILEGES ON DATABASE doko_analog_staging TO doko_app;

-- analytics schema for dbt output (prod)
\c doko_analog
CREATE SCHEMA IF NOT EXISTS analytics;
GRANT ALL ON SCHEMA analytics TO doko_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA analytics GRANT ALL ON TABLES TO doko_app;

-- analytics schema for dbt output (staging)
\c doko_analog_staging
CREATE SCHEMA IF NOT EXISTS analytics;
GRANT ALL ON SCHEMA analytics TO doko_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA analytics GRANT ALL ON TABLES TO doko_app;
