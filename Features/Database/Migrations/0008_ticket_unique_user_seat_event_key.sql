ALTER TABLE tickets
ADD CONSTRAINT tickets_user_id_seat_id_event_id_unique_key
UNIQUE (user_id, seat_id, event_id);
