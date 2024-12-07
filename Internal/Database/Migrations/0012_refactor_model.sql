ALTER TABLE users
ADD COLUMN first_name TEXT NOT NULL,
ADD COLUMN middle_name TEXT,
ADD COLUMN last_name TEXT NOT NULL,
ADD COLUMN birth_date DATE NOT NULL,
ADD COLUMN sex sex NOT NULL;

DROP TABLE user_details;

ALTER TABLE venues
DROP COLUMN capacity;

ALTER TABLE seats
DROP COLUMN status;
