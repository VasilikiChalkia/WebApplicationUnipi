UPDATE [dbo].[TableName]
   SET ColumnName = BulkColumn from Openrowset(Bulk 'Image_Path', Single_Blob) as ColumnName
 WHERE Id = 1
GO

-- To add an image to db as varBinary you must first remove all enters and tabs (make the image single line) 
-- and change also --> ( " ) (double quotes) to (single) --> ( ' )