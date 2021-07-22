#!/bin/bash

apply()
{
	echo Setting mode $1
	game --server=$hostname --user-key=$user --password=$password world hook $world --file $1.json
}

apply fish-fast
sleep 5m
apply fish-extra
sleep 5m
apply fish-standard
sleep 1s

apply dauds-extra
sleep 5m
apply dauds-standard
sleep 1

apply shields-none
sleep 5m
apply shields-standard
sleep 5m
apply shields-extra
sleep 5m
apply shields-standard
sleep 5m

apply obstacles-none
sleep 1
apply obstacles-fast
sleep 5m
apply obstacles-extra
sleep 5m
apply obstacles-standard
sleep 5m

apply robotguns-extra
sleep 5m
apply robotguns-standard
sleep 5m
