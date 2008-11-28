set-alias vmilasm "$(resolve-path ..\VM\VMILAssembler\bin\Debug\vmilasm.exe)" -scope Global
set-alias vmildasm "$(resolve-path ..\VM\VMILDisassembler\bin\Debug\vmildasm.exe)" -scope Global
set-alias vmshl "$(resolve-path ..\VM\VMShell\bin\Debug\vmshell.exe)" -scope Global

function global:test($name) {
	 if (-not (test-path "$name.vmil.bak")) {
	    write-error "No such test"
	 } else {
	 	 write-progress $name "Copying"
	 	 cp "$name.vmil.bak" "$name.vmil"
	 	 write-progress $name "Running"
	 	 vmshl "$name.vmil"
	}
}