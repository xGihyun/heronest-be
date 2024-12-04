ALTER TABLE events
DROP COLUMN venue_id;

ALTER TABLE event_dates RENAME TO event_occurrences;

ALTER TABLE event_occurrences 
ADD CONSTRAINT event_occurrences_venue_id_fkey 
FOREIGN KEY(venue_id) REFERENCES venues(venue_id);
