CREATE TYPE ticket_status AS ENUM('reserved', 'used', 'canceled');
CREATE TYPE seat_status AS ENUM('reserved', 'available', 'unavailable');

CREATE TABLE IF NOT EXISTS student_details (
    student_detail_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    student_number TEXT NOT NULL UNIQUE,
    year_level TEXT NOT NULL,
    section TEXT NOT NULL,

    user_id UUID NOT NULL UNIQUE,

    FOREIGN KEY(user_id) REFERENCES users(user_id)
);

CREATE TABLE IF NOT EXISTS venues (
    venue_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    name TEXT NOT NULL,
    description TEXT,
    capacity INT NOT NULL,
    location TEXT NOT NULL,
    image_url TEXT
);

CREATE TABLE IF NOT EXISTS events (
    event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    name TEXT NOT NULL,
    description TEXT,
    start_at TIMESTAMPTZ NOT NULL,
    end_at TIMESTAMPTZ NOT NULL,

    venue_id UUID NOT NULL,

    FOREIGN KEY(venue_id) REFERENCES venues(venue_id)
);

CREATE TABLE IF NOT EXISTS seat_sections (
    seat_section_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    name TEXT NOT NULL,
    description TEXT
);

CREATE TABLE IF NOT EXISTS seats (
    seat_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    seat_number TEXT NOT NULL,
    status seat_status NOT NULL,

    seat_section_id UUID,
    venue_id UUID NOT NULL,

    FOREIGN KEY(seat_section_id) REFERENCES seat_sections(seat_section_id),
    FOREIGN KEY(venue_id) REFERENCES venues(venue_id)
);

CREATE TABLE IF NOT EXISTS tickets (
    ticket_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    ticket_number TEXT NOT NULL UNIQUE,
    status ticket_status NOT NULL,
    metadata JSONB,

    user_id UUID NOT NULL,
    event_id UUID NOT NULL,
    seat_id UUID NOT NULL,

    FOREIGN KEY(user_id) REFERENCES users(user_id),
    FOREIGN KEY(event_id) REFERENCES events(event_id),
    FOREIGN KEY(seat_id) REFERENCES seats(seat_id)
);


