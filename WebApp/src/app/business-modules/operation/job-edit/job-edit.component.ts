import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import moment from 'moment/moment';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import * as shipmentHelper from 'src/helper/shipment.helper';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as dataHelper from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { ActivatedRoute, Router } from '@angular/router';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { NgForm } from '@angular/forms';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import filter from 'lodash/filter';
import cloneDeep from 'lodash/cloneDeep';
import { SortService } from 'src/app/shared/services/sort.service';
import { OpsModuleCreditDebitNoteDetailComponent } from './credit-debit-note/ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { OpsModuleCreditDebitNoteEditComponent } from './credit-debit-note/ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
declare var $: any;

@Component({
    selector: 'app-ops-module-billing-job-edit',
    templateUrl: './job-edit.component.html',
    styleUrls: ['./job-edit.component.scss']
})
export class OpsModuleBillingJobEditComponent implements OnInit {

    @ViewChild(OpsModuleCreditDebitNoteDetailComponent, { static: false }) poupDetail: OpsModuleCreditDebitNoteDetailComponent;
    @ViewChild(OpsModuleCreditDebitNoteEditComponent, { static: false }) popupEdit: OpsModuleCreditDebitNoteEditComponent;
    opsTransaction: OpsTransaction = null;
    productServices: any[] = [];
    serviceDate: any;
    finishDate: any;
    exchangeRateDate: any;
    serviceModes: any[] = [];
    shipmentModes: any[] = [];
    customers: any[] = [];
    ports: any[] = [];
    suppliers: any[] = [];
    agents: any[] = [];
    billingOps: any[] = [];
    warehouses: any[] = [];
    salemans: any[] = [];
    productServiceActive: any[] = [];
    serviceModeActive: any[] = [];
    shipmentModeActive: any[] = [];
    searchcontainer: string = '';
    lstMasterContainers: any[] = [];

    lstBuyingRateChargesComboBox: any[] = [];
    lstSellingRateChargesComboBox: any[] = [];
    lstOBHChargesComboBox: any[] = [];
    lstPartners: any[] = [];
    lstUnits: any[] = [];
    lstCurrencies: any[] = [];

    ListBuyingRateCharges: any[] = [];
    ConstListBuyingRateCharges: any = [];
    numberOfTimeSaveContainer: number = 0;

    ListSellingRateCharges: any[] = [];
    ConstListSellingRateCharges: any[] = [];

    ListOBHCharges: any[] = [];
    ConstListOBHCharges: any[] = [];

    BuyingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
    SellingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
    OBHChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();

    isDisplay: boolean = true;
    BuyingRateChargeToEdit: CsShipmentSurcharge = null;
    SellingRateChargeToEdit: CsShipmentSurcharge = null;
    OBHChargeToEdit: any = null;

    totalSellingUSD: number = 0;
    totalSellingLocal: number = 0;

    totalProfitUSD: number = 0;
    totalProfitLocal: number = 0;

    totalLogisticChargeUSD: number = 0;
    totalLogisticChargeLocal: number = 0;

    totalBuyingUSD: number = 0;
    totalBuyingLocal: number = 0;

    totalOBHUSD: number = 0;
    totalOBHLocal: number = 0;

    listContainerType: any[] = [];

    lstContainerTemp: any[];
    containerTypes: any[];
    listPackageTypes: any[];
    listWeightMesurement: any[];
    commodities: any[];
    packageTypes: any[] = [];
    weightMesurements: any[];
    currentActiveItemDefault: { id: null, text: null }[] = [];
    buyingRateChargeActive = [];
    sellingRateChargeActive = [];
    obhChargeActive = [];
    @ViewChild('containerMasterForm', { static: true }) containerMasterForm: NgForm;
    // @ViewChild('containerSelect',{static:true}) containerSelect: ElementRef;

    tab: string = '';
    jobId: string = '';

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU,
        private route: ActivatedRoute,
        private router: Router,
        private sortService: SortService) {
        this.keepCalendarOpeningWithRange = true;
        // this.selectedDate = Date.now();
        // this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }
    async ngOnInit() {

        this.route.params.subscribe(async (params: any) => {
            this.tab = 'job-edit';
            this.getUnits();
            this.getPartners();
            this.getCurrencies();
            this.getListBuyingRateCharges();
            this.getListSellingRateCharges();
            this.getListOBHCharges();
            this.getCustomers();
            this.getPorts();
            this.getSuppliers();
            this.getAgents();
            this.getBillingOps();
            this.getWarehouses();
            this.getContainerData();
            await this.getShipmentCommonData();
            if (!!params && !!params.id) {
                this.jobId = params.id;
                await this.getShipmentDetails(params.id);
                if (this.opsTransaction != null) {
                    this.serviceDate = (this.opsTransaction.serviceDate !== null) ? { startDate: moment(this.opsTransaction.serviceDate), endDate: moment(this.opsTransaction.serviceDate) } : null;
                    this.finishDate = this.opsTransaction.finishDate != null ? { startDate: moment(this.opsTransaction.finishDate), endDate: moment(this.opsTransaction.finishDate) } : null;
                    let index = this.productServices.findIndex(x => x.id === this.opsTransaction.productService);
                    if (index > -1) { this.productServiceActive = [this.productServices[index]]; }
                    index = this.serviceModes.findIndex(x => x.id === this.opsTransaction.serviceMode);
                    if (index > -1) { this.serviceModeActive = [this.serviceModes[index]] };
                    index = this.shipmentModes.findIndex(x => x.id === this.opsTransaction.shipmentMode);
                    if (index > -1) { this.shipmentModeActive = [this.shipmentModes[index]] };
                    this.getAllSurCharges();
                    this.getShipmentContainer();
                    this.getCustomClearances();
                } else {
                    this.serviceDate = null;
                    this.finishDate = null;
                }
            }
        });

    }

    async getShipmentContainer() {
        const responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsMawbcontainer.query, { mblid: this.opsTransaction.id }, false, false);
        this.opsTransaction.csMawbcontainers = this.lstContainerTemp = this.lstMasterContainers = responses;
    }
    async confirmDelete() {
        const respone = await this.baseServices.getAsync(this.api_menu.Documentation.Operation.checkAllowDelete + this.opsTransaction.id, false, true);
        if (respone === true) {
            $('#confirm-delete-job-modal').modal('show');
        } else {
            $('#confirm-can-not-delete-job-modal').modal('show');
        }
    }
    async deleteJob() {
        const respone = await this.baseServices.deleteAsync(this.api_menu.Documentation.Operation.delete + this.opsTransaction.id, true, true);
        if (respone.status) {
            $('#confirm-delete-job-modal').modal('hide');
            this.router.navigate(["/home/operation/job-management"]);
        }
    }
    async saveShipment(form: NgForm) {
        console.log(this.opsTransaction);
        this.opsTransaction.serviceDate = this.serviceDate.startDate != null ? dataHelper.dateTimeToUTC(this.serviceDate.startDate) : null;
        this.opsTransaction.finishDate = this.finishDate.startDate != null ? dataHelper.dateTimeToUTC(this.finishDate.startDate) : null;
        const s = this.finishDate.startDate != null && this.serviceDate.startDate != null && (this.finishDate.startDate < this.serviceDate.startDate);
        if (form.invalid || this.opsTransaction.shipmentMode == null
            || (this.opsTransaction.pod === this.opsTransaction.pol && this.opsTransaction.pod != null && this.opsTransaction.pol != null)
            || this.opsTransaction.serviceMode == null
            || this.opsTransaction.productService == null
            || this.opsTransaction.customerId == null
            || this.opsTransaction.billingOpsId == null
            || this.opsTransaction.serviceDate == null
            || s
            || this.opsTransaction.sumGrossWeight === 0
            || this.opsTransaction.sumNetWeight === 0
            || this.opsTransaction.sumCbm === 0
            || this.opsTransaction.sumPackages === 0
            || this.opsTransaction.sumContainers === 0
        ) {
            return;
        } else {
            this.opsTransaction.sumGrossWeight = this.opsTransaction.sumGrossWeight != null ? Number(this.opsTransaction.sumGrossWeight.toFixed(2)) : null;
            this.opsTransaction.sumNetWeight = this.opsTransaction.sumNetWeight != null ? Number(this.opsTransaction.sumNetWeight.toFixed(2)) : null;
            this.opsTransaction.sumCbm = this.opsTransaction.sumCbm != null ? Number(this.opsTransaction.sumCbm.toFixed(2)) : null;
            await this.baseServices.putAsync(this.api_menu.Documentation.Operation.update, this.opsTransaction, true, true);
            await this.getShipmentDetails(this.opsTransaction.id);
        }
    }
    // -------------     Container   ------------------- //
    /**
     * Show popup & init new first row( if container list is null)
     */
    showListContainer() {
        if (this.lstMasterContainers.length == 0) {
            this.lstMasterContainers.push(this.initNewContainer());
        }
        $('#container-list-of-ops-billing-modal').modal('show');
        $("#containerSelect").click();
    }

    /**
     * Close popup, assign list container to current job & remove last row that is not saved
     */
    closeContainerPopup() {
        const index = this.lstMasterContainers.findIndex(x => x.isNew === true);
        if (index > -1 && this.lstMasterContainers.length > 1) {
            this.lstMasterContainers.splice(index, 1);
        }
        $('#container-list-of-ops-billing-modal').modal('hide');
        this.opsTransaction.csMawbcontainers = this.lstMasterContainers;
    }

    /**
     * Add new row
     * 1. check existed a row that is editing
     * 2. change editing item to label row & push new row
     */
    addNewContainer() {
        let hasItemEdited = false;
        let index = -1;
        // 1.
        for (let i = 0; i < this.lstMasterContainers.length; i++) {
            if (this.lstMasterContainers[i].allowEdit == true) {
                hasItemEdited = true;
                index = i;
                break;
            }
        }
        // 2.
        if (hasItemEdited === false) {
            this.lstMasterContainers.push(this.initNewContainer());
        } else {
            this.saveNewContainer(index);
            if (this.saveContainerSuccess) {
                this.lstMasterContainers.push(this.initNewContainer());
            }
        }
    }

    /**
     * search container in list container
     * @param keySearch key to search in container list
     */
    searchContainer(keySearch: any) {
        keySearch = keySearch != null ? keySearch.trim().toLowerCase() : "";
        this.lstMasterContainers = Object.assign([], this.lstContainerTemp).filter(
            item => (item.containerTypeName.toLowerCase().includes(keySearch)
                || (item.quantity != null ? item.quantity.toString() : "").includes(keySearch)
                || (item.containerNo != null ? item.containerNo.toLowerCase() : "").includes(keySearch)
                || (item.sealNo != null ? item.sealNo.toLowerCase() : "").includes(keySearch)
                || (item.markNo != null ? item.markNo.toLowerCase() : "").includes(keySearch)
                || (item.commodityName != null ? item.commodityName.toLowerCase() : "").includes(keySearch)
                || (item.packageTypeName != null ? item.packageTypeName.toLowerCase() : "").includes(keySearch)
                || (item.packageQuantity != null ? item.packageQuantity.toString().toLowerCase() : "").includes(keySearch)
                || (item.description != null ? item.description.toLowerCase() : "").includes(keySearch)
                || (item.nw != null ? item.nw.toString().toLowerCase() : "").includes(keySearch)
                || (item.chargeAbleWeight != null ? item.chargeAbleWeight.toString() : "").toLowerCase().includes(keySearch)
                || (item.gw != null ? item.gw.toString().toLowerCase() : "").includes(keySearch)
                || (item.unitOfMeasureName != null ? item.unitOfMeasureName.toLowerCase() : "").includes(keySearch)
                || (item.cbm != null ? item.cbm.toString().toLowerCase() : "").includes(keySearch)
            )
        );
    }

    /**
     * change mode of row(editing - not editing)
     */
    containerToChangeEdit: any = null;
    changeEditMode(index: any) {
        this.getComboboxDataContainer(index);
        if (this.lstMasterContainers[index].allowEdit === false || this.lstMasterContainers[index].allowEdit == undefined) {
            this.lstMasterContainers[index].allowEdit = true;
            this.lstMasterContainers[index].containerTypeActive = this.lstMasterContainers[index].containerTypeId != null ?
                [{ id: this.lstMasterContainers[index].containerTypeId, text: this.lstMasterContainers[index].containerTypeName }] : [];
            this.lstMasterContainers[index].packageTypeActive = this.lstMasterContainers[index].packageTypeId != null ?
                [{ id: this.lstMasterContainers[index].packageTypeId, text: this.lstMasterContainers[index].packageTypeName }] : [];
            this.lstMasterContainers[index].unitOfMeasureActive = this.lstMasterContainers[index].unitOfMeasureID != null ?
                [{ id: this.lstMasterContainers[index].unitOfMeasureID, text: this.lstMasterContainers[index].unitOfMeasureName }] : [];
            for (let i = 0; i < this.lstMasterContainers.length; i++) {
                if (i !== index) {
                    this.lstMasterContainers[i].allowEdit = false;
                }
            }
        } else {
            this.lstMasterContainers[index].allowEdit = false;
        }
        this.containerToChangeEdit = Object.assign({}, this.lstMasterContainers[index]);
    }

    saveContainerSuccess = false;
    /**
     * change a row from editing mode in table
     * @param index index of row in table
     */
    async saveNewContainer(index: any) {
        if (this.lstMasterContainers[index].containerNo.length > 0 || this.lstMasterContainers[index].sealNo.length > 0 || this.lstMasterContainers[index].markNo.length > 0) {
            this.lstMasterContainers[index].quantity = 1;
        }
        this.lstMasterContainers[index].verifying = true;
        if (this.lstMasterContainers[index].quantity == null
            || this.lstMasterContainers[index].containerTypeId == null
            || this.lstMasterContainers[index].gw === 0 || this.lstMasterContainers[index].nw === 0
            || this.lstMasterContainers[index].chargeAbleWeight === 0
            || this.lstMasterContainers[index].cbm === 0) {
            this.saveContainerSuccess = false;
            return;
        }
        // Check duplicate( Cont Type && Cont Q'ty && Container No && Package Type)
        let existedItems = this.lstMasterContainers.filter(x => x.containerTypeId == this.lstMasterContainers[index].containerTypeId
            && x.quantity == this.lstMasterContainers[index].quantity);
        if (this.lstMasterContainers[index].containerNo.length != 0 && this.lstMasterContainers[index].packageTypeId != null) {
            existedItems = existedItems.filter(x => x.containerNo == this.lstMasterContainers[index].containerNo
                && x.packageTypeId == this.lstMasterContainers[index].packageTypeId);
        } else {
            existedItems = [];
        }
        // set row is invalid
        if (existedItems.length > 1) {
            this.lstMasterContainers[index].inValidRow = true;
            this.saveContainerSuccess = false;
        } else {
            // set row is saved
            if (this.lstMasterContainers[index].isNew === true) {
                this.lstMasterContainers[index].isNew = false;
            } else {
                this.lstMasterContainers[index].inValidRow = false;
                this.lstMasterContainers[index].containerTypeActive = this.lstMasterContainers[index].containerTypeId != null ? [{ id: this.lstMasterContainers[index].containerTypeId, text: this.lstMasterContainers[index].containerTypeName }] : [];
                this.lstMasterContainers[index].packageTypeActive = this.lstMasterContainers[index].packageTypeId != null ? [{ id: this.lstMasterContainers[index].packageTypeId, text: this.lstMasterContainers[index].packageTypeName }] : [];
                this.lstMasterContainers[index].unitOfMeasureActive = this.lstMasterContainers[index].unitOfMeasureId != null ? [{ id: this.lstMasterContainers[index].unitOfMeasureId, text: this.lstMasterContainers[index].unitOfMeasureName }] : [];
            }
            this.roundWeight(index);
            this.saveContainerSuccess = true;
            this.lstMasterContainers[index].allowEdit = false;
        }
        this.lstContainerTemp = Object.assign([], this.lstMasterContainers);
    }
    roundWeight(index: number) {
        if (this.lstMasterContainers[index].gw != null) {
            this.lstMasterContainers[index].gw = Number(this.lstMasterContainers[index].gw.toFixed(2));
        }
        if (this.lstMasterContainers[index].nw != null) {
            this.lstMasterContainers[index].nw = Number(this.lstMasterContainers[index].nw.toFixed(2));
        }
        if (this.lstMasterContainers[index].cbm != null) {
            this.lstMasterContainers[index].cbm = Number(this.lstMasterContainers[index].cbm.toFixed(2));
        }
        if (this.lstMasterContainers[index].chargeAbleWeight != null) {
            this.lstMasterContainers[index].chargeAbleWeight = Number(this.lstMasterContainers[index].chargeAbleWeight.toFixed(2));
        }
    }
    /**
     * save list container
     */
    async onSubmitContainer() {
        for (let i = 0; i < this.lstMasterContainers.length; i++) {
            this.lstMasterContainers[i].verifying = true;
        }
        if (this.containerMasterForm.valid) {
            let hasItemEdited = false;
            for (let i = 0; i < this.lstMasterContainers.length; i++) {
                if (this.lstMasterContainers[i].allowEdit == true) {
                    hasItemEdited = true;
                    break;
                }
            }
            if (hasItemEdited === false) {
                // continue
                this.numberOfTimeSaveContainer = this.numberOfTimeSaveContainer + 1;
                this.opsTransaction.sumGrossWeight = 0;
                this.opsTransaction.sumNetWeight = 0;
                this.opsTransaction.sumChargeWeight = 0;
                this.opsTransaction.sumCbm = 0;
                this.opsTransaction.sumContainers = 0;
                this.opsTransaction.sumPackages = 0;
                this.opsTransaction.csMawbcontainers = this.lstMasterContainers;
                var response = await this.baseServices.putAsync(this.api_menu.Documentation.CsMawbcontainer.update, { csMawbcontainerModels: this.opsTransaction.csMawbcontainers, masterId: this.opsTransaction.id }, true, false);
                if (response.status) {
                    this.getGoodInfomation(this.lstMasterContainers);
                    await this.baseServices.putAsync(this.api_menu.Documentation.Operation.update, this.opsTransaction, false, false);
                    $('#container-list-of-ops-billing-modal').modal('hide');
                }
            }
            else {
                this.baseServices.errorToast("Current container must be save!!!");
            }
        }
    }

    /**
     * cancel edit stage of container row
     * @param index 
     */
    cancelNewContainer(index: number) {
        if (this.lstMasterContainers[index].isNew === true) {
            this.lstMasterContainers.splice(index, 1);
        } else {
            this.lstMasterContainers[index] = this.containerToChangeEdit;
            this.lstMasterContainers[index].allowEdit = false;
            this.containerToChangeEdit = null;
        }

    }

    /**
     * get container information of a job
     * @param listContainers list of container
     */
    getGoodInfomation(listContainers) {
        for (var i = 0; i < listContainers.length; i++) {
            listContainers[i].isSave = true;
            this.opsTransaction.sumGrossWeight = this.opsTransaction.sumGrossWeight + listContainers[i].gw;
            this.opsTransaction.sumNetWeight = this.opsTransaction.sumNetWeight + listContainers[i].nw;
            this.opsTransaction.sumChargeWeight = this.opsTransaction.sumChargeWeight + listContainers[i].chargeAbleWeight;
            this.opsTransaction.sumCbm = this.opsTransaction.sumCbm + listContainers[i].cbm;
            this.opsTransaction.sumContainers = this.opsTransaction.sumContainers + listContainers[i].quantity;
            this.opsTransaction.sumPackages = this.opsTransaction.sumPackages + listContainers[i].packageQuantity;
        }
    }
    /**
     * init a new container
     */
    initNewContainer() {
        const container = {
            mawb: this.opsTransaction.id,
            containerTypeId: null,
            containerTypeName: '',
            containerTypeActive: [],
            quantity: 1,
            containerNo: '',
            sealNo: '',
            markNo: '',
            unitOfMeasureId: 37,
            unitOfMeasureName: 'Kilogam',
            unitOfMeasureActive: [], ///[{ "id": 37, "text": "Kilogam" }],
            commodityId: null,
            commodityName: '',
            packageTypeId: null,
            packageTypeName: '',
            packageTypeActive: [],
            packageQuantity: null,
            description: null,
            gw: null,
            nw: null,
            chargeAbleWeight: null,
            cbm: null,
            packageContainer: '',
            allowEdit: true,
            isNew: true,
            verifying: false
        };
        this.getComboboxDataContainer(this.lstMasterContainers.length);
        return container;
    }
    removeContainer() {
        this.lstMasterContainers.splice(this.indexItemConDelete, 1);
        this.opsTransaction.csMawbcontainers = this.lstMasterContainers;
        $('#confirm-accept-delete-container-modal').modal('hide');
    }
    indexItemConDelete: any = null;
    /**
     * confirm item to remove out of table
     * @param index index of row in table
     */
    removeAContainer(index: number) {
        this.indexItemConDelete = index;
    }
    /**
     * get container type data
     */
    async getContainerTypes() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Container", inactive: false }, false, false);
        this.listContainerType = responses;
        if (responses != null) {
            this.containerTypes = dataHelper.prepareNg2SelectData(responses, 'id', 'unitNameEn');
        }
    }
    /**
     * get reference container
     */
    getContainerData() {
        this.getContainerTypes();
        this.getWeightTypes();
        this.getPackageTypes();
        this.getComodities();
    }

    /**
     * get list weight type
     */
    async getWeightTypes() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Weight Measurement", inactive: false }, false, false);
        this.listWeightMesurement = responses;
    }

    /**
     * get list package type
     */
    async getPackageTypes() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Package", inactive: false }, false, false);
        this.listPackageTypes = responses;

        if (this.listPackageTypes) {
            this.packageTypes = dataHelper.prepareNg2SelectData(this.listPackageTypes, 'id', 'unitNameEn');
            this.packagesUnitActive = [this.packageTypes[0]];
        }
    }
    /**
     * get list commodity
     */
    async getComodities() {
        let criteriaSearchCommodity = { inactive: null, all: null };
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Commodity.query, criteriaSearchCommodity, false, false);
        if (responses != null) {
            this.commodities = this.sortService.sort(responses, 'commodityNameEn', true);
        }
        else {
            this.commodities = [];
        }
    }
    getComboboxDataContainer(index: number) {
        if (this.listContainerType != null) {
            if (this.lstMasterContainers[index] == null) {
                this.containerTypes = dataHelper.prepareNg2SelectData(this.listContainerType, 'id', 'unitNameEn');
                this.packageTypes = dataHelper.prepareNg2SelectData(this.listPackageTypes, 'id', 'unitNameEn');
                this.weightMesurements = dataHelper.prepareNg2SelectData(this.listWeightMesurement, 'id', 'unitNameEn');
            } else {
                if (this.lstMasterContainers[index]["id"] != null) {
                    let conts = this.listContainerType.filter(x => x.inactive === false);
                    let packs = this.listPackageTypes.filter(x => x.inactive === false);
                    let weights = this.listWeightMesurement.filter(x => x.inactive === false);
                    this.containerTypes = dataHelper.prepareNg2SelectData(conts, 'id', 'unitNameEn');
                    this.packageTypes = dataHelper.prepareNg2SelectData(packs, 'id', 'unitNameEn');
                    this.weightMesurements = dataHelper.prepareNg2SelectData(weights, 'id', 'unitNameEn');
                }
            }
        }
        else {
            this.containerTypes = [];
        }
    }
    // -------------    End Container   -------------------//

    async getWarehouses() {
        this.baseServices.post(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Warehouse, inactive: false }).subscribe((res: any) => {
            this.warehouses = res;
        });
    }
    async getShipmentDetails(id: any) {
        this.opsTransaction = await this.baseServices.getAsync(this.api_menu.Documentation.Operation.getById + "?id=" + id, false, true);
        this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
    }
    async getShipmentCommonData() {
        const data = await shipmentHelper.getOPSShipmentCommonData(this.baseServices, this.api_menu);
        this.productServices = dataHelper.prepareNg2SelectData(data.productServices, 'value', 'displayName');
        this.serviceModes = dataHelper.prepareNg2SelectData(data.serviceModes, 'value', 'displayName');
        this.shipmentModes = dataHelper.prepareNg2SelectData(data.shipmentModes, 'value', 'displayName');
    }
    private getListBillingOps() {
        this.baseServices.get(this.api_menu.System.User_Management.getAll).subscribe((res: any) => {
            this.billingOps = res;
        });
    }
    private getPorts() {
        this.baseServices.post(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Port, inactive: false }).subscribe((res: any) => {
            this.ports = res;
        });
    }

    private getCustomers() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER, all: null }).subscribe((res: any) => {
            this.customers = res;
        });
    }
    private getSuppliers() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CARRIER, inactive: false, all: null }).subscribe((res: any) => {
            this.suppliers = res;
        });
    }
    private getAgents() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.AGENT, inactive: false, all: null }).subscribe((res: any) => {
            this.agents = res;
        });
    }
    private getBillingOps() {
        this.baseServices.get(this.api_menu.System.User_Management.getAll).subscribe((res: any) => {
            this.billingOps = res;
            this.salemans = res;
        });
    }

    public getListBuyingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'CREDIT', serviceTypeId: 'SEF' }).subscribe(res => {
            this.lstBuyingRateChargesComboBox = res['data'];
        });

    }

    public getListSellingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'DEBIT', serviceTypeId: 'SEF' }).subscribe(res => {
            this.lstSellingRateChargesComboBox = res['data'];
        });
    }

    public getListOBHCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'OBH', serviceTypeId: 'SEF' }).subscribe(res => {
            this.lstOBHChargesComboBox = res['data'];
        });
    }

    public getPartners() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.ALL, inactive: false }).subscribe((res: any) => {
            this.lstPartners = res;
            console.log({ PARTNERS: this.lstPartners });
        });
    }

    public getUnits() {
        this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, { inactive: false }).subscribe((data: any) => {
            this.lstUnits = data;
        });
    }

    public getCurrencies(isAddNew = true) {
        if (isAddNew === true) {
            this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { inactive: false }).subscribe((res: any) => {
                this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
            });
        } else {
            this.baseServices.get(this.api_menu.Catalogue.Currency.getAll).subscribe((res: any) => {
                this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
            });
        }
    }

    calculateTotalEachBuying(isEdit: boolean = false) {
        let total = 0;
        if (isEdit) {
            if (this.BuyingRateChargeToEdit.vatrate >= 0) {
                total = this.BuyingRateChargeToEdit.quantity * this.BuyingRateChargeToEdit.unitPrice * (1 + (this.BuyingRateChargeToEdit.vatrate / 100));
            } else {
                total = this.BuyingRateChargeToEdit.quantity * this.BuyingRateChargeToEdit.unitPrice + Math.abs(this.BuyingRateChargeToEdit.vatrate);
            }
            this.BuyingRateChargeToEdit.total = Number(total.toFixed(2));
        } else {
            if (this.BuyingRateChargeToAdd.vatrate >= 0) {
                total = this.BuyingRateChargeToAdd.quantity * this.BuyingRateChargeToAdd.unitPrice * (1 + (this.BuyingRateChargeToAdd.vatrate / 100));
            } else {
                total = this.BuyingRateChargeToAdd.quantity * this.BuyingRateChargeToAdd.unitPrice + Math.abs(this.BuyingRateChargeToAdd.vatrate);
            }
            this.BuyingRateChargeToAdd.total = Number(total.toFixed(2));
        }
    }

    calculateTotalEachSelling(isEdit: boolean = false) {
        let total = 0;
        if (isEdit) {
            if (this.SellingRateChargeToEdit.vatrate >= 0) {
                total = this.SellingRateChargeToEdit.quantity * this.SellingRateChargeToEdit.unitPrice * (1 + (this.SellingRateChargeToEdit.vatrate / 100));
            } else {
                total = this.SellingRateChargeToEdit.quantity * this.SellingRateChargeToEdit.unitPrice + Math.abs(this.SellingRateChargeToEdit.vatrate);
            }
            this.SellingRateChargeToEdit.total = Number(total.toFixed(2));
        } else {
            if (this.SellingRateChargeToAdd.vatrate >= 0) {
                total = this.SellingRateChargeToAdd.quantity * this.SellingRateChargeToAdd.unitPrice * (1 + (this.SellingRateChargeToAdd.vatrate / 100));
            } else {
                total = this.SellingRateChargeToAdd.quantity * this.SellingRateChargeToAdd.unitPrice + Math.abs(this.SellingRateChargeToAdd.vatrate);
            }
            this.SellingRateChargeToAdd.total = Number(total.toFixed(2));
        }
    }


    calculateTotalEachOBH(isEdit: boolean = false) {
        let total = 0;
        if (isEdit) {
            if (this.OBHChargeToEdit.vatrate >= 0) {
                total = this.OBHChargeToEdit.quantity * this.OBHChargeToEdit.unitPrice * (1 + (this.OBHChargeToEdit.vatrate / 100));
            } else {
                total = this.OBHChargeToEdit.quantity * this.OBHChargeToEdit.unitPrice + Math.abs(this.OBHChargeToEdit.vatrate);
            }
            this.OBHChargeToEdit.total = total;
        } else {
            if (this.OBHChargeToAdd.vatrate >= 0) {
                total = this.OBHChargeToAdd.quantity * this.OBHChargeToAdd.unitPrice * (1 + (this.OBHChargeToAdd.vatrate / 100));
            } else {
                total = this.OBHChargeToAdd.quantity * this.OBHChargeToAdd.unitPrice + Math.abs(this.OBHChargeToAdd.vatrate);
            }
            this.OBHChargeToAdd.total = total;
        }
    }

    resetDisplay() {
        this.isDisplay = false;
        setTimeout(() => {
            this.isDisplay = true;
        }, 50);
    }

    saveNewCharge(id_form: string, form: NgForm, data: CsShipmentSurcharge, isContinue: boolean) {
        setTimeout(async () => {
            const error = $('#' + id_form).find('div.has-danger');
            if (error.length === 0) {
                data.hblid = this.opsTransaction.hblid;
                if (data.quantity != null) {
                    data.quantity = Number(data.quantity.toFixed(2));
                }
                const res = await this.baseServices.postAsync(this.api_menu.Documentation.CsShipmentSurcharge.addNew, data);
                if (res.status) {
                    form.onReset();
                    this.resetDisplay();
                    this.getAllSurCharges();
                    this.BuyingRateChargeToAdd = new CsShipmentSurcharge();
                    this.SellingRateChargeToAdd = new CsShipmentSurcharge();
                    this.OBHChargeToAdd = new CsShipmentSurcharge();
                    this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
                    this.baseServices.setData("ShipmentAdded", true);
                    if (!isContinue) {
                        $('#' + id_form).modal('hide');
                    }
                }
            }
        }, 300);
    }



    private totalProfit() {
        this.totalProfitUSD = this.totalSellingUSD - this.totalBuyingUSD - this.totalLogisticChargeUSD;
        this.totalProfitLocal = this.totalSellingLocal - this.totalBuyingLocal - this.totalLogisticChargeLocal;
    }

    /**
     * Calculate total cost for all buying charges 
     */
    private totalBuyingCharge() {
        this.totalBuyingUSD = 0;
        this.totalBuyingLocal = 0;
        if (this.ListBuyingRateCharges.length > 0) {

            this.ListBuyingRateCharges.forEach(element => {

                this.totalBuyingLocal += element.total * element.exchangeRate;
                this.totalBuyingUSD += this.totalBuyingLocal / element.exchangeRateUSDToVND;
                this.totalProfit();
            });
        }
    }

    /**
    * Calculate total cost for all selling charges 
    */
    private totalSellingCharge() {
        this.totalSellingUSD = 0;
        this.totalSellingLocal = 0;
        if (this.ListSellingRateCharges.length > 0) {

            this.ListSellingRateCharges.forEach(element => {
                this.totalSellingLocal += element.total * element.exchangeRate;
                this.totalSellingUSD += this.totalSellingLocal / element.exchangeRateUSDToVND;
                this.totalProfit();

            });

        }
    }

    /**
    * Calculate total cost for all obh charges 
    */
    private totalOBHCharge() {
        this.totalOBHUSD = 0;
        this.totalOBHLocal = 0;
        if (this.ListOBHCharges.length > 0) {

            this.ListOBHCharges.forEach(element => {

                this.totalOBHLocal += element.total * element.exchangeRate;
                this.totalOBHUSD += this.totalOBHLocal / element.exchangeRateUSDToVND;
                this.totalProfit();
            });

        }
    }

    getSurCharges(type: 'BUY' | 'SELL' | 'OBH') {
        this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getByHBId + "?hbId=" + this.opsTransaction.hblid + "&type=" + type).subscribe((res: any) => {
            if (type === 'BUY') {
                this.ListBuyingRateCharges = res;
                this.ConstListBuyingRateCharges = res;
                this.totalBuyingCharge();
                console.log(this.ListBuyingRateCharges);
            }
            if (type === 'SELL') {
                this.ListSellingRateCharges = res;
                this.ConstListSellingRateCharges = res;
                this.totalSellingCharge();
            }
            if (type === 'OBH') {
                this.ListOBHCharges = res;
                this.ConstListOBHCharges = res;
                this.totalOBHCharge();
            }
        });
    }
    getAllSurCharges() {
        this.getSurCharges('BUY');
        this.getSurCharges('SELL');
        this.getSurCharges('OBH');
    }



    prepareEditCharge(type: 'BUY' | 'SELL' | 'OBH', charge: any) {
        if (type === 'BUY') {
            this.BuyingRateChargeToEdit = cloneDeep(charge);
            this.buyingRateChargeActive = [{'text': this.BuyingRateChargeToEdit.currency, 'id': this.BuyingRateChargeToEdit.currencyId}]
            //this.BuyingRateChargeToEdit.exchangeDate = { startDate: moment(this.BuyingRateChargeToEdit.exchangeDate), endDate: moment(this.BuyingRateChargeToEdit.exchangeDate) };
            if (this.BuyingRateChargeToEdit.exchangeDate != null) {
                this.exchangeRateDate = { startDate: moment(this.BuyingRateChargeToEdit.exchangeDate), endDate: moment(this.BuyingRateChargeToEdit.exchangeDate) };
            }
        }
        if (type === 'SELL') {
            this.SellingRateChargeToEdit = cloneDeep(charge);
            this.sellingRateChargeActive = [{'text': this.SellingRateChargeToEdit.currency,' id': this.SellingRateChargeToEdit.currencyId}];
            if (this.SellingRateChargeToEdit.exchangeDate != null) {
                this.exchangeRateDate = { startDate: moment(this.SellingRateChargeToEdit.exchangeDate), endDate: moment(this.SellingRateChargeToEdit.exchangeDate) };
            }
            //this.SellingRateChargeToEdit.exchangeDate = { startDate: moment(this.SellingRateChargeToEdit.exchangeDate), endDate: moment(this.SellingRateChargeToEdit.exchangeDate) };

        }
        if (type === 'OBH') {
            this.OBHChargeToEdit = cloneDeep(charge);
            this.obhChargeActive = [{'text': this.OBHChargeToEdit.currency,'id':this.OBHChargeToEdit.currencyId}];
            //this.OBHChargeToEdit.exchangeDate = { startDate: moment(this.OBHChargeToEdit.exchangeDate), endDate: moment(this.OBHChargeToEdit.exchangeDate) };
            if (this.OBHChargeToEdit.exchangeDate != null) {
                this.exchangeRateDate = { startDate: moment(this.OBHChargeToEdit.exchangeDate), endDate: moment(this.OBHChargeToEdit.exchangeDate) };
            }
        }
    }

    chargeIdToDelete: string = null;
    async DeleteCharge(stt: string, chargeId: string = null) {
        if (stt == "confirm") {
            console.log(chargeId);
            this.chargeIdToDelete = chargeId;
        }
        if (stt == "ok") {
            var res = await this.baseServices.deleteAsync(this.api_menu.Documentation.CsShipmentSurcharge.delete + "?chargId=" + this.chargeIdToDelete);
            if (res.status) {
                this.getAllSurCharges();
            }

        }
    }

    CDNoteDetails: AcctCDNoteDetails = null;
    async openCreditDebitNote(soaNo: string) {
        this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + this.opsTransaction.id + "&soaNo=" + soaNo);
        if (this.CDNoteDetails != null) {
            if (this.CDNoteDetails.listSurcharges != null) {
                this.totalCreditDebitCalculate();
            }
            this.poupDetail.show({ backdrop: 'static' });
        }
    }

    openEditCDNotePopUp(event) {
        this.CDNoteDetails = null;
        console.log(event);
        if (event != null) {
            this.CDNoteDetails = event;
            //this.baseServices.setData("CDNoteDetails", event);
            this.popupEdit.show({ backdrop: 'static' });
        }
    }
    async closeEditModal(event) {
        console.log(event);
        this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + this.opsTransaction.id + "&soaNo=" + this.CDNoteDetails.cdNote.code);
        if (this.CDNoteDetails != null) {
            if (this.CDNoteDetails.listSurcharges != null) {
                this.totalCreditDebitCalculate();
            }
            this.poupDetail.show({ backdrop: 'static' });
        }
    }
    totalCreditDebitCalculate() {
        let totalCredit = 0;
        let totalDebit = 0;
        for (let i = 0; i < this.CDNoteDetails.listSurcharges.length; i++) {
            const c = this.CDNoteDetails.listSurcharges[i];
            if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.payerId)) {
                // calculate total credit
                totalCredit += (c.total * c.exchangeRate);
            }
            if (c.type === "SELL" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.receiverId)) {
                // calculate total debit 
                totalDebit += (c.total * c.exchangeRate);
            }

        }
        this.CDNoteDetails.totalCredit = totalCredit;
        this.CDNoteDetails.totalDebit = totalDebit;
    }


    searchCharge(key: string, type: 'BUY' | 'SELL' | 'OBH') {
        const search_key = key.toString().trim().toLowerCase();
        var referenceData: any[] = [];
        if (type === 'BUY') {
            referenceData = this.ConstListBuyingRateCharges;
        }
        if (type === 'SELL') {
            referenceData = this.ConstListSellingRateCharges;
        }
        if (type === 'OBH') {
            referenceData = this.ConstListOBHCharges;
        }
        var results = filter(referenceData, function (x: any) {
            return (
                ((x.partnerName == null ? "" : x.partnerName.toLowerCase().includes(search_key)) ||
                    (x.nameEn == null ? "" : x.nameEn.toLowerCase().includes(search_key)) ||
                    (x.unit == null ? "" : x.unit.toLowerCase().includes(search_key)) ||
                    (x.currency == null ? "" : x.currency.toLowerCase().includes(search_key)) ||
                    (x.notes == null ? "" : x.notes.toLowerCase().includes(search_key)) ||
                    (x.docNo == null ? "" : x.docNo.toLowerCase().includes(search_key)) ||
                    (x.quantity == null ? "" : x.quantity.toString().toLowerCase().includes(search_key)) ||
                    (x.unitPrice == null ? "" : x.unitPrice.toString().toLowerCase().includes(search_key)) ||
                    (x.vatrate == null ? "" : x.vatrate.toString().toLowerCase().includes(search_key)) ||
                    (x.total == null ? "" : x.total.toString().toLowerCase().includes(search_key)))
            )
        });

        return results;
    }

    editCharge(id_form: string, form: NgForm, data: CsShipmentSurcharge) {
        setTimeout(async () => {
            if (form.submitted) {
                const error = $('#' + id_form).find('div.has-danger');
                if (error.length === 0) {
                    if (data.quantity != null) {
                        data.quantity = Number(data.quantity.toFixed(2));
                    }
                    if (this.exchangeRateDate != null) {
                        data.exchangeDate = this.exchangeRateDate.startDate != null ? dataHelper.dateTimeToUTC(this.exchangeRateDate.startDate) : null;
                    }
                    const res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update, data);
                    if (res.status) {
                        $('#' + id_form).modal('hide');
                        this.getAllSurCharges();
                        this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
                        this.baseServices.setData("ShipmentUpdated", true);
                    }
                }
            }
        }, 300);
    }


    closeChargeForm(formId: string, form: NgForm) {
        form.onReset();
        this.resetDisplay();
        $('#' + formId).modal("hide");

        this.currentActiveItemDefault = [];
        this.BuyingRateChargeToAdd = new CsShipmentSurcharge();
        this.SellingRateChargeToAdd = new CsShipmentSurcharge();
        this.OBHChargeToAdd = new CsShipmentSurcharge();

        this.BuyingRateChargeToEdit = null;
        this.SellingRateChargeToEdit = null;
        this.OBHChargeToEdit = null;

    }

    /**
       * Daterange picker
       */
    //selectedRange: any;
    //selectedDate: any;
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
    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4',
        'option 5', 'option 6', 'option 7'];


    //packagesUnit: Array<string> = ['PKG', 'PCS', 'BOX', 'CNTS'];
    packagesUnitActive = ['PKG'];

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

    /**
     * get custom clearances
     */
    getCustomClearances() {
        this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
    }

    selectTab($event: any, tabName: string) {
        this.tab = tabName;
        this.getAllSurCharges();
        // this.router.navigate([`home/operation/job-edit/${this.jobId}`], {queryParams: {tab: this.tab}});
    }
}
