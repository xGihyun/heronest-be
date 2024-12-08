ALTER TABLE tickets
DROP COLUMN event_occurrence_id;

ALTER TABLE tickets
ADD COLUMN event_id UUID NOT NULL;

ALTER TABLE tickets
ADD CONSTRAINT tickets_event_id_fkey
FOREIGN KEY(event_id) REFERENCES events(event_id);

DROP TABLE event_occurrences;

ALTER TABLE events
ADD COLUMN start_at TIMESTAMPTZ NOT NULL,
ADD COLUMN end_at TIMESTAMPTZ NOT NULL,
ADD COLUMN image_url TEXT,
ADD COLUMN venue_id UUID NOT NULL;

ALTER TABLE events
ADD CONSTRAINT events_venue_id_fkey
FOREIGN KEY(venue_id) REFERENCES venues(venue_id);
