# Note: these values may only change during major release

If ($Version.Contains('-')) {

	# Use the development keys
	$Keys = @{
	    'netstandard1.1' = '2bc5e20c3ad68793'
		'net452' = '2bc5e20c3ad68793'
	}

} Else {

	# Use the final release keys
	$Keys = @{
		'netstandard1.1' = 'c80e32baaec95a4f'
		'net452' = 'c80e32baaec95a4f'
	}

}

function Resolve-FullPath() {
	param([string]$Path)
	[System.IO.Path]::GetFullPath((Join-Path (pwd) $Path))
}
