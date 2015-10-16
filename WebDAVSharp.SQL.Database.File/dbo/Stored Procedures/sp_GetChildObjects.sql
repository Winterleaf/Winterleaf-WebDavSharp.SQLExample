
CREATE proc sp_GetChildObjects
	@ObjectGuid as uniqueidentifier = null
as
	create table #temp
	(
	
	objectguid uniqueidentifier ,
	isCollection bit

	)

	if @ObjectGuid is null
	begin
	select @ObjectGuid = pk_folderid from folder where fk_ParentFolderId is null
	end

	insert into #temp select @ObjectGuid,1

	declare @count int, @lastCount int

	set @lastCount = 0
	set @count = 1

	while (@lastCount <> @count)
	begin
		set @lastCount = @count
		insert into #temp (objectguid,isCollection)
		select pk_folderid,convert(bit,1) from [folder] where fk_ParentFolderId in (select objectguid from #temp) and pk_FolderId not in (select objectguid from #temp)
		select @count = count(*) from #temp
	end

	insert into #temp(objectguid,isCollection)
	select pk_FileId,convert(bit,0) from [file] where fk_FolderId in (select objectguid from #temp where isCollection =1)

	select objectguid,isCollection from #temp

	drop table #temp
	

