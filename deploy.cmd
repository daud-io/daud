call heroku login
call heroku container:login

call heroku container:push web --app %1
call heroku container:release web --app %1
