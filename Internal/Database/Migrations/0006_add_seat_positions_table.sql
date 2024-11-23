CREATE TABLE IF NOT EXISTS seat_positions (
    seat_position_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    x INT NOT NULL,
    y INT NOT NULL,

    seat_id UUID NOT NULL UNIQUE,

    FOREIGN KEY(seat_id) REFERENCES seats(seat_id)
);

