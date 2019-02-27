for /l %%x in (1, 1, 100000) do (
   echo %%x

	call apply fish-fast
	sleep 300
	call apply fish-extra
	sleep 300
	call apply fish-standard
	sleep 1

	call apply dauds-extra
	sleep 300
	call apply dauds-standard
	sleep 1

	call apply shields-none
	sleep 300
	call apply shields-standard
	sleep 300
	call apply shields-extra
	sleep 300
	call apply shields-standard
	sleep 300

	call apply obstacles-fast
	sleep 300
	call apply obstacles-extra
	sleep 300
	call apply obstacles-standard
	sleep 300

	call apply robotguns-extra
	sleep 300
	call apply robotguns-standard
	sleep 300

)
