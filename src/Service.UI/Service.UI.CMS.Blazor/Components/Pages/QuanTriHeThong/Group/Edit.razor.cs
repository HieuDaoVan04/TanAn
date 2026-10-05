// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Extensions;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.Group
{
    public partial class Edit : ComponentBase
    {
        [CascadingParameter]
        public FluentDialog Dialog { get; set; } = default!;

        [Parameter]
        public EditOrUpdateParametersGroupDto Content { get; set; } = new();

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;

        private string ErrorMessage { get; set; } = string.Empty;
        private bool IsLoading { get; set; } = true;
        private bool IsSaving { get; set; } = false;
        private GroupsForm GroupsForm { get; set; } = new();
        private string SelectedUnitType { get; set; } = "1";
        private string SelectedUnitGroup { get; set; } = "1";
        private GroupsDto? DonViDaChon = null;
        private List<SelectOption> UnitTypeOptions = new();
        private List<SelectOption> UnitGroupOptions = new();

        public class SelectOption
        {
            public int Value { get; set; }
            public string Text { get; set; } = string.Empty;
        }

        public class OptionItem<T>
        {
            public string Text { get; set; } = string.Empty;
            public T Value { get; set; } = default!;
        }

        protected override void OnInitialized()
        {
            UnitTypeOptions.AddRange(
                Enum.GetValues<OrganizationUnitType>()
                    .Select(e => new SelectOption
                    {
                        Value = (int)e,
                        Text = GetEnumDisplayName(e)
                    })
            );
            UnitGroupOptions.AddRange(
                Enum.GetValues<OrganizationUnitGroup>()
                    .Select(e => new SelectOption
                    {
                        Value = (int)e,
                        Text = GetEnumDisplayName(e)
                    })
            );
        }

        private async Task OnSearchSelect(OptionsSearchEventArgs<GroupsDto> e)
        {
            string input = e.Text;
            e.Items = await FetchDataGroupParent(input);
        }

        private async Task<List<GroupsDto>> FetchDataGroupParent(string searchKeyword)
        {
            var output = new List<GroupsDto>();

            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/Groups/GetPaged"
                };

                var baseQuery = new GroupsQuery
                {
                    draw = 1,
                    SearchIn = new List<string> { "Name" },
                    Keyword = searchKeyword,
                    IsDonVi = true,
                    gridRequest = new GridRequest
                    {
                        filter = new Filter(),
                        page = 1,
                        pageSize = 20,
                    }
                };

                ResultAPI<DataTableJson<GroupsDto>> result = await CallService.Post<DataTableJson<GroupsDto>>(apiRequest, baseQuery);

                if (result.Status == StatusCode.OK && result.Data is DataTableJson<GroupsDto> dataTable)
                {
                    output = dataTable.Data.ToList();

                    if (Content.Object is not null && !output.Any(e => e.Id == Content.Object.Id))
                    {
                        output.Insert(0, Content.Object);
                    }

                    if (DonViDaChon is not null && !output.Any(e => e.Id == DonViDaChon.Id))
                    {
                        output.Insert(0, DonViDaChon);
                    }
                }
            }
            catch (Exception)
            {
                await ShowErrorMessage("Có lỗi khi gọi api lấy dữ liệu đơn vị trực thuộc");
            }

            return output;
        }

        private string GetEnumDisplayName<T>(T enumValue) where T : Enum
        {
            return SIConvert.GetEnumDescription(enumValue);
        }

        private void OnUnitTypeChanged()
        {
            GroupsForm.UnitType = (OrganizationUnitType)Convert.ToInt32(SelectedUnitType);
        }

        private void OnUnitGroupChanged()
        {
            GroupsForm.UnitGroup = (OrganizationUnitGroup)Convert.ToInt32(SelectedUnitGroup);
        }

        protected override async Task OnAfterRenderAsync(bool firtsLoad)
        {
            if (firtsLoad)
            {
                try
                {
                    if (Content.IsEditMode && Content.Id != Guid.Empty)
                    {
                        GroupsForm = await FetchDataById(Content.Id);
                        SelectedUnitType = ((int)GroupsForm.UnitType).ToString();
                        SelectedUnitGroup = ((int)GroupsForm.UnitGroup).ToString();

                        if (GroupsForm.ParentId.HasValue)
                        {
                            DonViDaChon = await LoadParentGroupById(GroupsForm.ParentId.Value);
                        }
                    }
                    else
                    {
                        GroupsForm = new();
                        if (Content.Object is not null)
                        {
                            GroupsForm.ParentId = Content.Object.Id;
                            DonViDaChon = Content.Object;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
                }
                finally
                {
                    IsLoading = false;
                }
                StateHasChanged();
            }
        }

        private async Task<GroupsDto?> LoadParentGroupById(Guid parentId)
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/Groups/{parentId}"
                };

                ResultAPI<GroupsDto> result = await CallService.Get<GroupsDto>(apiRequest);
                if (result.Status == StatusCode.OK && result.Data != null)
                {
                    return result.Data;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        private async Task<GroupsForm> FetchDataById(Guid Id)
        {
            ApiRequestModel apiRequest = new ApiRequestModel()
            {
                ApiService = ServicesRegistryEnum.ServicePortal,
                Endpoint = $"/Groups/{Id}"
            };

            ResultAPI<GroupsForm> result = await CallService.Get<GroupsForm>(apiRequest);
            if (result.Status != StatusCode.OK || result.Data is null)
            {
                await ShowErrorMessage("Có lỗi khi gọi api lấy dữ liệu");
                return new();
            }
            return result.Data;
        }

        private async Task Save()
        {
            IsSaving = true;
            try
            {
                if (DonViDaChon is not null)
                    GroupsForm.ParentId = DonViDaChon.Id;
                else GroupsForm.ParentId = null;

                if (Content.Id == Guid.Empty)
                {
                    ApiRequestModel apiRequest = new ApiRequestModel()
                    {
                        ApiService = ServicesRegistryEnum.ServicePortal,
                        Endpoint = $"/Groups"
                    };
                    if (Content.IsAddGroup) GroupsForm.UnitType = OrganizationUnitType.DonVi;
                    ResultAPI<string> result = await CallService.Post<string>(apiRequest, GroupsForm);

                    if (result.Status != StatusCode.OK)
                    {
                        ToastService.ShowWarning($"Lỗi: {result.Message ?? "Đã xảy ra lỗi khi thực hiện thao tác"}");
                    }
                    else
                    {
                        ToastService.ShowSuccess("Thêm mới thành công!");
                        await Dialog.CloseAsync();
                        await Content.OnRefresh.InvokeAsync();
                    }
                }
                else
                {
                    ApiRequestModel apiRequest = new ApiRequestModel()
                    {
                        ApiService = ServicesRegistryEnum.ServicePortal,
                        Endpoint = $"/Groups/{Content.Id}"
                    };
                    ResultAPI<object> result = await CallService.Put<object>(apiRequest, GroupsForm);

                    if (result.Status != StatusCode.OK)
                    {
                        ToastService.ShowWarning("Cập nhật thất bại" + result.Message);
                    }
                    else
                    {
                        ToastService.ShowSuccess("Cập nhật thành công!");
                        await Dialog.CloseAsync();
                        await Content.OnRefresh.InvokeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowErrorMessage(ex.Message);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private async Task ShowErrorMessage(string? content)
        {
            await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
            {
                Content = new()
                {
                    Title = "Lỗi",
                    MarkupMessage = new MarkupString($"Có lỗi: {content}"),
                    Icon = new Icons.Regular.Size24.ErrorCircle(),
                    IconColor = Color.Error,
                },
                PrimaryAction = "OK",
                PrimaryActionEnabled = true,
            });
        }

        private async Task CancelAsync()
        {
            await Dialog.CancelAsync();
        }
    }
}
