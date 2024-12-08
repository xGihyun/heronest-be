ALTER TABLE events 
DROP COLUMN start_at, 
DROP COLUMN end_at;

CREATE TABLE IF NOT EXISTS event_dates (
    event_date_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    start_at TIMESTAMPTZ NOT NULL,
    end_at TIMESTAMPTZ NOT NULL,

    event_id UUID NOT NULL,
    venue_id UUID NOT NULL,

    FOREIGN KEY(event_id) REFERENCES events(event_id)
);
