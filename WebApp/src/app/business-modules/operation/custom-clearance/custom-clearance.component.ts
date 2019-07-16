import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
//import { SystemConstants } from 'src/constants/system.const';
import * as lodash from 'lodash';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
declare var $: any;

@Component({
    selector: 'app-custom-clearance',
    templateUrl: './custom-clearance.component.html',
    styleUrls: ['./custom-clearance.component.scss']
})
export class CustomClearanceComponent implements OnInit {
    listCustomDeclaration: any = [];
    pager: PagerSetting = PAGINGSETTING;
    searchObject: any = {};
    listUser: Array<string> = [];
    clearanceNo: string = '';
    customCheckedArray: any = [];
    listCustomer: any = [];
    listPort: any = [];
    listUnit: any = [];

    constructor(
        private excelService: ExcelService,
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService,
        private router: Router,
        private toastr: ToastrService) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        //this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
        this.selectedRange = { startDate: moment().subtract(30, 'days'), endDate: moment() };
    }

    async ngOnInit() {
        this.initPager();
        this.getListUser();
        this.currentUser = [localStorage.getItem('currently_userName')];
        this.getListCustomsDeclaration();
        this.getListCustomer();
        this.getListPort();
        this.getListUnit();
    }

    initPager(): any {
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;

        this.customCheckedArray = [];
    }

    async getListCustomsDeclaration() {
        const res = await this.baseServices.postAsync(this.api_menu.Operation.CustomClearance.paging + "?pageNumber=" + this.pager.currentPage + "&pageSize=" + this.pager.pageSize, this.searchObject, true, true);
        console.log(res);
        this.listCustomDeclaration = res.data;
        this.pager.totalItems = res.totalItems;

        this.customCheckedArray = [];
    }

    getListUser() {
        this.baseServices.get(this.api_menu.System.User_Management.getAll).subscribe((res: any) => {
            this.listUser = res.map(x => ({ "text": x.username, "id": x.id }));
        }, err => {
            this.listUser = [];
            this.baseServices.handleError(err);
        });
    }

    setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.pageSize = pager.pageSize;
        this.pager.totalPages = pager.totalPages;
        this.getListCustomsDeclaration();
    }

    async searchCustomClearance() {
        this.initPager();
        this.searchObject = {};

        this.searchObject.ClearanceNo = this.clearanceNo;
        this.searchObject.FromClearanceDate = this.selectedRange.startDate._d;
        this.searchObject.ToClearanceDate = this.selectedRange.endDate._d;
        if (this.defaultImportStatus[0] === 'All') {
            this.searchObject.ImPorted = null;
        } else {
            this.searchObject.ImPorted = this.defaultImportStatus[0] === 'Imported' ? true : false;
        }
        this.searchObject.FromImportDate = this.selectedRangeImportDate.startDate ? this.selectedRangeImportDate.startDate._d : null;
        this.searchObject.ToImportDate = this.selectedRangeImportDate.endDate ? this.selectedRangeImportDate.endDate._d : null;
        if (this.defaultTypeClearance[0] !== 'All') {
            this.searchObject.Type = this.defaultTypeClearance[0];
        } else {
            this.searchObject.Type = null;
        }

        this.searchObject.PersonHandle = this.currentUser[0].toString();
        console.log(this.searchObject);
        this.getListCustomsDeclaration();
    }

    async resetSearch() {
        this.clearanceNo = '';
        this.selectedRange = { startDate: moment().subtract(30, 'days'), endDate: moment() };
        this.selectedRangeImportDate = null;
        this.defaultImportStatus = ['Not imported'];
        this.defaultTypeClearance = ['All'];
        this.currentUser = [localStorage.getItem('currently_userName')];
        this.searchObject = {};
        this.initPager();
        this.getListCustomsDeclaration();
    }

    isDesc = true;
    sortKey: string = "";
    sort(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.listCustomDeclaration = this.sortService.sort(this.listCustomDeclaration, property, this.isDesc);
    }

    gotoEditPage(id) {
        this.router.navigate(["/home/operation/custom-clearance-edit", { id: id }]);
    }
    async getDataFromEcus() {
        await this.baseServices.postAsync(this.api_menu.Operation.CustomClearance.importClearancesFromEcus, null, true, true);
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;
        this.getListCustomsDeclaration();
    }

    onChangeAction(custom, isChecked: boolean) {
        if (isChecked) {
            this.customCheckedArray.push(custom);
        } else {
            let index = this.customCheckedArray.indexOf(custom);
            this.customCheckedArray.splice(index, 1);
        }
        console.log(this.customCheckedArray);
    }
    confirmConvert() {
        if (this.customCheckedArray.length > 0) {
            $('#confirm-convert-modal').modal('show');
        }
        else {
            this.toastr.warning('Not selected custom clearance');
        }
    }
    confirmDelete() {
        if (this.customCheckedArray.length > 0) {
            $('#btnDeleteCustomClearance').attr('data-target', '#confirm-delete-modal');
        } else {
            $('#btnDeleteCustomClearance').removeAttr('data-target');
            this.toastr.warning('Not selected custom clearance', '', { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 5000 });
        }
    }

    async delete() {
        const response = await this.baseServices.putAsync(this.api_menu.Operation.CustomClearance.deleteMultiple, this.customCheckedArray, true, true);
        console.log(response);
        await this.initPager();
        await this.getListCustomsDeclaration();
    }

    async cancelDelete() {

    }

    async convertToJobs() {
        let clearancesToConvert = this.mapClearancesToJobs();
        let response = await this.baseServices.postAsync(this.api_menu.Documentation.Operation.convertExistedClearancesToJobs, clearancesToConvert, true, true);
        if (response.status) {
            await this.initPager();
            await this.getListCustomsDeclaration();
        }
    }
    async getListCustomer() {
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER }, true, true);
        this.listCustomer = res;
    }
    async getListPort() {
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Port }, true, true);
        this.listPort = res;
    }
    async getListUnit() {
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: 'Package' }, true, true);
        this.listUnit = res;
    }
    mapClearancesToJobs() {
        let clearancesToConvert = [];
        for (let i = 0; i < this.customCheckedArray.length; i++) {
            let clearance = this.customCheckedArray[i];
            let shipment = new OpsTransaction();
            let index = this.listCustomer.findIndex(x => x.taxCode == clearance.partnerTaxCode);
            if (index > -1) {
                let customer = this.listCustomer[index];
                shipment.customerId = customer.id;
                shipment.salemanId = customer.salemanId;
                index = this.listPort.findIndex(x => x.code == clearance.gateway);
                if (index > -1) {
                    if (clearance.type == "Export") {
                        shipment.pol = this.listPort[index].id;
                    }
                    if (clearance.type == "Import") {
                        shipment.pod = this.listPort[index].id;
                    }
                }
                if (clearance.serviceType == "Sea") {
                    if (clearance.cargoType == "FCL") {
                        shipment.productService = "Sea FCL";
                    }
                    if (clearance.cargoType == "LCL") {
                        shipment.productService = "Sea LCL";
                    }
                }
                else {
                    shipment.productService = clearance.serviceType;
                }
                shipment.shipmentMode = "External";
                shipment.mblno = clearance.mblid;
                shipment.hwbno = clearance.hblid;
                shipment.serviceDate = clearance.clearanceDate;
                shipment.sumGrossWeight = clearance.grossWeight;
                shipment.sumNetWeight = clearance.netWeight;
                shipment.sumCbm = clearance.cbm;
                let claim = localStorage.getItem('id_token_claims_obj');
                let currenctUser = JSON.parse(claim)["id"];
                shipment.billingOpsId = currenctUser;
                index = this.listUnit.findIndex(x => x.code == clearance.unitCode);
                if (index > -1) {
                    shipment.packageTypeID = this.listUnit[index].id;
                }
            }
            else {
                this.baseServices.errorToast("Không đủ điều kiện để tạo job mới");
                shipment = null;
            }
            if (clearance.clearanceDate == null) {
                this.baseServices.errorToast("Không đủ điều kiện để tạo job mới");
                shipment = null;
            }
            clearancesToConvert.push({ opsTransaction: shipment, customsDeclaration: clearance });
        }
        return clearancesToConvert;
    }

    async export() {
        /**Prepare data */
        var customClearances = await this.baseServices.postAsync(this.api_menu.Operation.CustomClearance.query, this.searchObject);
        console.log(customClearances);
        //if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
        customClearances = lodash.map(customClearances, function (item, index) {
            return [
                index + 1,
                item.clearanceNo,
                item.type,
                item.gateway,
                item.customerName,
                item.importCountryName,
                item.exportCountryName,
                item.jobNo,
                moment(item.clearanceDate).format('DD/MM/YYYY'),
                (item.jobNo != null && item.jobNo != '') ? 'Imported' : 'Not Imported'
            ]
        });
        //}
        // if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
        //     customClearances = lodash.map(customClearances, function (item, index) {
        //         return [
        //             index + 1,
        //             item.clearanceNo,
        //             item.type,
        //             item.gateway,
        //             item.customerName,
        //             item.importCountryName,
        //             item.exportCountryName,
        //             item.jobNo,
        //             moment(item.clearanceDate).format('DD/MM/YYYY'),
        //             (item.jobNo != null && item.jobNo != '') ? 'Imported' : 'Not Imported'
        //         ]
        //     });
        // }

        /**Set up stylesheet */
        const exportModel: ExportExcel = new ExportExcel();
        exportModel.fileName = "Custom Clearance Report";
        const currrently_user = localStorage.getItem('currently_userName');
        exportModel.title = "Custom Clearance Report ";
        exportModel.author = currrently_user;
        exportModel.header = [
            { name: "No.", width: 10 },
            { name: "Clearance No", width: 25 },
            { name: "Type", width: 10 },
            { name: "Gateway", width: 25 },
            { name: "Partner Name", width: 25 },
            { name: "Import Country", width: 25 },
            { name: "Export Country", width: 25 },
            { name: "JOBID", width: 25 },
            { name: "Clearance Date", width: 20 },
            { name: "Status", width: 25 }
        ];

        exportModel.data = customClearances;
        this.excelService.generateExcel(exportModel);
    }

    /**
     * Daterange picker
     */
    selectedRange: any;
    selectedRangeImportDate: any;
    selectedDate: any;
    keepCalendarOpeningWithRange: true;
    maxDate: moment.Moment = moment();
    ranges: any = {
        Today: [moment(), moment()],
        Yesterday: [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
        'Last 7 Days': [moment().subtract(6, 'days'), moment()],
        'Last 30 Days': [moment().subtract(29, 'days'), moment()],
        'This Month': [moment().startOf('month'), moment().endOf('month')],
        'Last Month': [
            moment()
                .subtract(1, 'month')
                .startOf('month'),
            moment()
                .subtract(1, 'month')
                .endOf('month')
        ]
    };

    /**
  * ng2-select
  */
    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4', 'option 5', 'option 6', 'option 7'];

    statusClearance: Array<string> = ['All', 'Imported', 'Not imported'];
    typeClearance: Array<string> = ['All', 'Export', 'Import'];
    userList: Array<string> = [];
    currentUser = ['Thor'];
    defaultImportStatus = ['Not imported'];
    defaultTypeClearance = ['All'];

    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;

    private get disabledV(): string {
        return this._disabledV;
    }

    private set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }

    public selected(value: any): void {
        console.log('Selected value is: ', value);
    }

    public removed(value: any): void {
        console.log('Removed value is: ', value);
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }

    public selectedImportStatus(value: any): void {
        console.log('selected Import Status value is: ', value);
        this.defaultImportStatus = [value.id];
    }

    public selectedTypeClearance(value: any): void {
        console.log('selected Type Clearance value is: ', value);
        this.defaultTypeClearance = [value.id];
    }

}
