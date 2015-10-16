CREATE PROCEDURE GetSecurityTokens @SecurityObjectId UNIQUEIDENTIFIER
AS
WITH MyCTE
AS (
	SELECT SecurityObjectId
	FROM SecurityObject
	WHERE SecurityObjectId = @SecurityObjectId
	
	UNION ALL
	
	SELECT MemberofSecurityobjectId
	FROM [SecurityObjectMembership]
	INNER JOIN MyCTE ON [SecurityObjectMembership].SecurityObjectId = MyCTE.SecurityObjectId
	)
SELECT DISTINCT SecurityObject.SecurityObjectId
FROM MyCTE
INNER JOIN SecurityObject ON MyCTE.SecurityObjectId = SecurityObject.SecurityObjectId
ORDER BY SecurityObjectId