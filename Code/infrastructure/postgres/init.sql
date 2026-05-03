-- Creates the staging database alongside the prod database.
-- Runs once on first container start (docker-entrypoint-initdb.d).
CREATE DATABASE doko_analog_staging;
GRANT ALL PRIVILEGES ON DATABASE doko_analog_staging TO doko_app;
