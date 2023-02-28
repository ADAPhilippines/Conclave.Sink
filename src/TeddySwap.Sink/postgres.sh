#!/bin/bash
docker run -d \
	--name test-postgres \
	-e POSTGRES_PASSWORD=test1234 \
	-e PGDATA=/var/lib/postgresql/data/pgdata \
	-v 1_data:/var/lib/postgresql/data \
    -p 5432:5432 \
	postgres