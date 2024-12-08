CREATE TYPE sex AS ENUM('male', 'female');
CREATE TYPE role AS ENUM('admin', 'staff', 'student', 'visitor');

CREATE TABLE IF NOT EXISTS users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    email TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    role role NOT NULL
);

CREATE TABLE IF NOT EXISTS user_details (
    user_detail_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,

    first_name TEXT NOT NULL,
    middle_name TEXT,
    last_name TEXT NOT NULL,
    birth_date DATE NOT NULL,
    sex sex NOT NULL,

    user_id UUID NOT NULL UNIQUE,

    FOREIGN KEY(user_id) REFERENCES users(user_id)
);
