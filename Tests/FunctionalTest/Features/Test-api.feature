Feature: FunctionalTest

@api
Scenario: Upload file to Blob
	Given client requests the file
	When send a upload request to the api
	Then operation is ok


@api
Scenario: Download file from Blob
	Given client requests the file
	When send a upload request to the api
	Then send a download request to the api
	Then operation is ok

@api
Scenario: Upload multiple file to Blob
	Given client requests the file
	When send multiple files to upload using the api
	Then operation is ok

@api
Scenario: Delete uploaded file from Blob
	Given client requests the file
	When send a upload request to the api
	Then delete the uploaded file
	Then operation is ok