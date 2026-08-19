using Application.SeedWork.Queries;
using Domain.Entities;

namespace Application.Cameras.Queries;

public sealed partial class DashboardCamerasQuery
{
    public static readonly TableQueryDefinition<Camera, DashboardCamerasQuery> Table = new(
        Filters:
        [
            TableFilterDescriptorExtensions.IntEquals<Camera, DashboardCamerasQuery>("numberId", query => query.NumberId, camera => camera.NumberId),
            TableFilterDescriptorExtensions.GuidEquals<Camera, DashboardCamerasQuery>("id", query => query.Id, camera => camera.Id),
            TableFilterDescriptor<Camera, DashboardCamerasQuery>.TextContains("name", query => query.Name, camera => camera.CameraName.Value),
            TableFilterDescriptor<Camera, DashboardCamerasQuery>.TextContains("brand", query => query.Brand, camera => camera.CameraBrand.Brand.ToString()),
            TableFilterDescriptor<Camera, DashboardCamerasQuery>.TextContains("model", query => query.Model, camera => camera.CameraBrand.Model),
            TableFilterDescriptor<Camera, DashboardCamerasQuery>.TextContains("ipAddress", query => query.IpAddress, camera => camera.IpAddress ?? string.Empty),
            TableFilterDescriptor<Camera, DashboardCamerasQuery>.TextContains("siteName", query => query.SiteName, camera => camera.Site!.Name.Value)
        ],
        Sorts: new Dictionary<string, TableSortDescriptor<Camera, DashboardCamerasQuery>>(StringComparer.OrdinalIgnoreCase)
        {
            ["numberId"] = TableSortDescriptor<Camera, DashboardCamerasQuery>.Create("numberId", camera => camera.NumberId, camera => camera.Id),
            ["id"] = TableSortDescriptor<Camera, DashboardCamerasQuery>.Create("id", camera => camera.Id),
            ["name"] = TableSortDescriptor<Camera, DashboardCamerasQuery>.Create("name", camera => camera.CameraName.Value, camera => camera.Id),
            ["brand"] = TableSortDescriptor<Camera, DashboardCamerasQuery>.Create("brand", camera => camera.CameraBrand.Brand, camera => camera.Id),
            ["model"] = TableSortDescriptor<Camera, DashboardCamerasQuery>.Create("model", camera => camera.CameraBrand.Model, camera => camera.Id),
            ["ipAddress"] = TableSortDescriptor<Camera, DashboardCamerasQuery>.Create("ipAddress", camera => camera.IpAddress ?? string.Empty, camera => camera.Id),
            ["siteName"] = TableSortDescriptor<Camera, DashboardCamerasQuery>.Create("siteName", camera => camera.Site!.Name.Value, camera => camera.Id)
        },
        DefaultSort: query => query.OrderBy(camera => camera.CameraName.Value).ThenBy(camera => camera.Id));
}
