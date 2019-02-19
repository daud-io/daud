#! /bin/bash
if ! heroku whoami; then
    heroku login
fi
heroku container:login
heroku labs:enable runtime-dyno-metadata -a $1
heroku container:push web -a $1
heroku container:release web -a $1
