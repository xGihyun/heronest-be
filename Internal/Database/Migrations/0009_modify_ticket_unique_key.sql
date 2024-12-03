ALTER TABLE tickets
DROP CONSTRAINT tickets_user_id_seat_id_event_id_unique_key,
ADD CONSTRAINT tickets_user_id_event_id_unique_key
UNIQUE (user_id, event_id);
