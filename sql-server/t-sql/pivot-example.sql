declare @countries nvarchar(max) = '';
select @countries = @countries + QUOTENAME(CountryName) + ',' from tblCountry
set @countries = LEFT(@countries, len(@countries)-1)

declare @sql nvarchar(max) = N'select top 10 * from (select c.CountryName, YEAR(f.FilmReleaseDate) as [Year] from tblCountry c inner join tblFilm f on c.CountryID = f.FilmCountryID
) as pivotData
pivot (
count(CountryName)
For [CountryName]
in (' + @countries + N')) as pivottable where [United States] != 0 and [United Kingdom] != 0'
exec sp_executesql @sql
