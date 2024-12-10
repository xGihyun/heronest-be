ALTER TABLE event_occurrences 
RENAME COLUMN event_date_id TO event_occurrence_id;

ALTER TABLE event_occurrences 
RENAME CONSTRAINT event_dates_pkey TO event_occurrences_pkey;

ALTER TABLE tickets
DROP COLUMN event_id;

ALTER TABLE tickets
ADD COLUMN event_occurrence_id UUID NOT NULL;

ALTER TABLE tickets 
ADD CONSTRAINT tickets_event_occurrence_id_fkey 
FOREIGN KEY(event_occurrence_id) REFERENCES event_occurrences(event_occurrence_id);
