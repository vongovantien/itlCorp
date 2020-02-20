import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import moment from 'moment/moment';
import { NgForm } from '@angular/forms';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import { Location } from '@angular/common';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services/sort.service';
import { CatalogueRepo, OperationRepo, DocumentationRepo } from 'src/app/shared/repositories';

@Component({
    selector: 'app-custom-clearance-addnew',
    templateUrl: './custom-clearance-addnew.component.html',
    styleUrls: ['./custom-clearance-addnew.component.scss']
})
export class CustomClearanceAddnewComponent implements OnInit {
    customDeclaration: CustomClearance = new CustomClearance();
    listCustomer: any = [];
    listPort: any = [];
    listCountry: any = [];
    listCommodity: any = [];
    listUnit: any = [];
    isConvertJob: boolean = false;

    constructor(private _location: Location,
        private cdr: ChangeDetectorRef,
        private sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
        private _operationRepo: OperationRepo,
        private _documentation: DocumentationRepo,
        private toastr: ToastrService
    ) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    ngOnInit() {
        this.getClearanceType();
        this.getListCustomer();
        this.getListPort();
        this.getListCountry();
        this.getListUnit();
        this.getListCommodity();
    }

    async addCustomClearance(formAdd: NgForm) {
        if (this.strCustomerCurrent == '' || this.strPortCurrent == '') return;
        if (this.serviceTypeCurrent[0] != 'Air' && this.serviceTypeCurrent[0] != 'Express') {
            if (this.cargoTypeCurrent.length == 0) return;
        }

        var pattern = /^[a-zA-Z0-9./_-\s]*$/;

        if (!this.customDeclaration.clearanceNo.match(pattern)) {
            this.toastr.warning("Clearance No - Not allowed to enter special characters. Except for characters ./_- and space", '', { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 5000 });
            return;
        }

        if (this.customDeclaration.hblid != null && !this.customDeclaration.hblid.toString().match(pattern)) {
            this.toastr.warning("HBL - Not allowed to enter special characters. Except for characters ./_- and space", '', { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 5000 });
            return;
        }

        if (this.customDeclaration.mblid != null && !this.customDeclaration.mblid.toString().match(pattern)) {
            this.toastr.warning("MBL - Not allowed to enter special characters. Except for characters ./_- and space", '', { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 5000 });
            return;
        }

        if (formAdd.form.status != "INVALID" && this.customDeclaration.clearanceDate.endDate != null) {
            this.cdr.detach();
            this.customDeclaration.partnerTaxCode = this.strCustomerCurrent;
            this.customDeclaration.clearanceDate = moment(this.customDeclaration.clearanceDate.endDate._d).format('YYYY-MM-DD');
            this.customDeclaration.serviceType = this.serviceTypeCurrent[0];
            this.customDeclaration.gateway = this.strPortCurrent;
            this.customDeclaration.type = this.typeClearanceCurrent[0];
            this.customDeclaration.route = this.routeClearanceCurrent[0];
            this.customDeclaration.cargoType = (this.serviceTypeCurrent[0] == 'Air' || this.serviceTypeCurrent[0] == 'Express') ? null : this.cargoTypeCurrent[0];
            this.customDeclaration.exportCountryCode = this.strCountryExportCurrent;
            this.customDeclaration.importCountryCode = this.strCountryImportCurrent;
            this.customDeclaration.commodityCode = this.strCommodityCurrent;
            this.customDeclaration.unitCode = this.strUnitCurrent;
            console.log(this.customDeclaration);

            // const respone = await this.baseServices.postAsync(this.api_menu.Operation.CustomClearance.add, this.customDeclaration, true, true);
            const respone = await this._operationRepo.addCustomDeclaration(this.customDeclaration).toPromise();
            console.log(respone);
            if (respone['status'] === true) {
                this.toastr.success(respone['message']);
                this._location.back();
            } else {
                // Reset lại clearanceDate
                this.customDeclaration.clearanceDate = { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };
            }
        }
    }

    async convertClearanceToShipment(formAdd: NgForm) {
        if (this.strCustomerCurrent === '' || this.strPortCurrent === '' || this.typeClearanceCurrent.length === 0
            || this.customDeclaration.hblid == null || this.customDeclaration.hblid === ''
            || this.customDeclaration.mblid == null || this.customDeclaration.mblid === '') { return; }
        if (this.serviceTypeCurrent[0] !== 'Air' && this.serviceTypeCurrent[0] !== 'Express') {
            if (this.cargoTypeCurrent.length === 0) { return; }
        }
        if (formAdd.form.status !== "INVALID" && this.customDeclaration.clearanceDate.endDate != null) {
            this.cdr.detach();
            this.customDeclaration.partnerTaxCode = this.strCustomerCurrent;
            this.customDeclaration.clearanceDate = moment(this.customDeclaration.clearanceDate.endDate._d).format('YYYY-MM-DD');
            this.customDeclaration.serviceType = this.serviceTypeCurrent[0];
            this.customDeclaration.gateway = this.strPortCurrent;
            this.customDeclaration.type = this.typeClearanceCurrent[0];
            this.customDeclaration.route = this.routeClearanceCurrent[0];
            this.customDeclaration.cargoType = (this.serviceTypeCurrent[0] === 'Air' || this.serviceTypeCurrent[0] === 'Express') ? null : this.cargoTypeCurrent[0];
            this.customDeclaration.exportCountryCode = this.strCountryExportCurrent;
            this.customDeclaration.importCountryCode = this.strCountryImportCurrent;
            this.customDeclaration.commodityCode = this.strCommodityCurrent;
            this.customDeclaration.unitCode = this.strUnitCurrent;
            console.log(this.customDeclaration);
            const shipment = this.mapClearanceToShipment();
            //const response = await this.baseServices.postAsync(this.api_menu.Documentation.Operation.convertClearanceToJob, { opsTransaction: shipment, customsDeclaration: this.customDeclaration }, true, true);
            const response = await this._documentation.convertClearanceToJob({ opsTransaction: shipment, customsDeclaration: this.customDeclaration }).toPromise();

            if (response.status) {
                this.isConvertJob = false;
                this._location.back();
            } else {
                this.isConvertJob = true;
                this.customDeclaration.clearanceDate = { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };
            }
        }
    }

    mapClearanceToShipment() {
        let shipment = new OpsTransaction();
        let index = this.listCustomer.findIndex(x => x.taxCode === this.customDeclaration.partnerTaxCode);
        if (index > -1) {
            const customer = this.listCustomer[index];
            shipment.customerId = customer.id;
            shipment.salemanId = customer.salePersonId;
            shipment.serviceMode = this.customDeclaration.type;
            index = this.listPort.findIndex(x => x.code === this.customDeclaration.gateway);
            if (index > -1) {
                if (this.customDeclaration.type === "Export") {
                    shipment.pol = this.listPort[index].id;
                    shipment.clearanceLocation = shipment.pol;
                }
                if (this.customDeclaration.type === "Import") {
                    shipment.pod = this.listPort[index].id;
                    shipment.clearanceLocation = shipment.pod;
                }
            }
            if (this.customDeclaration.serviceType === "Sea") {
                if (this.customDeclaration.cargoType === "FCL") {
                    shipment.productService = "SeaFCL";
                }
                if (this.customDeclaration.cargoType === "LCL") {
                    shipment.productService = "SeaLCL";
                }
            } else {
                shipment.productService = this.customDeclaration.serviceType;
            }
            shipment.shipmentMode = "External";
            shipment.mblno = this.customDeclaration.mblid;
            shipment.hwbno = this.customDeclaration.hblid;
            shipment.serviceDate = this.customDeclaration.clearanceDate;
            shipment.sumGrossWeight = this.customDeclaration.grossWeight;
            shipment.sumNetWeight = this.customDeclaration.netWeight;
            shipment.sumCbm = this.customDeclaration.cbm;
            const claim = localStorage.getItem('id_token_claims_obj');
            const currenctUser = JSON.parse(claim)["id"];
            shipment.billingOpsId = currenctUser;
            index = this.listUnit.findIndex(x => x.code === this.customDeclaration.unitCode);
            if (index > -1) {
                shipment.packageTypeId = this.listUnit[index].id;
            }
        } else {
            this.toastr.error("Không có customer để tạo job mới");
            shipment = null;
        }
        if (this.customDeclaration.clearanceDate == null) {
            this.toastr.error("Không có clearance date để tạo job mới");
            shipment = null;
        }
        return shipment;
    }

    getListCustomer() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CUSTOMER })
            .subscribe((res: any) => { this.listCustomer = res; });
    }

    getClearanceType() {
        this._operationRepo.getClearanceType()
            .subscribe(
                (res: any) => {
                    this.serviceTypes = res.serviceTypes.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.typeClearance = res.types.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.routeClearance = res.routes.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.cargoTypes = res.cargoTypes.map(x => ({ "text": x.displayName, "id": x.value }));
                }
            );
    }

    getListPort() {
        this._catalogueRepo.getListPort({ placeType: PlaceTypeEnum.Port })
            .subscribe((res: any) => { this.listPort = res; });
    }

    getListCountry() {
        this._catalogueRepo.getListAllCountry()
            .subscribe((res: any) => { this.listCountry = res; });
    }

    getListCommodity() {
        this._catalogueRepo.getCommondity({})
            .subscribe((res: any) => { this.listCommodity = res; });
    }

    getListUnit() {
        this._catalogueRepo.getUnit({ unitType: 'Package' })
            .subscribe((res: any) => {
                this.listUnit = res;
                const datasort = this.sortService.sort(res, 'code', true);
                this.strUnitCurrent = datasort != null ? datasort[0].code : '';
            });
    }

    selectedRange: any;
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
    serviceTypes: any = [];
    typeClearance: any = [];
    routeClearance: any = [];
    cargoTypes: any = [];

    strCustomerCurrent: any = '';
    strPortCurrent: any = '';
    strCountryImportCurrent: any = '';
    strCountryExportCurrent: any = '';
    strCommodityCurrent: any = '';
    strUnitCurrent: any = '';

    serviceTypeCurrent: any = [];
    typeClearanceCurrent: any = [];
    routeClearanceCurrent: any = [];
    cargoTypeCurrent: any = [];

    private _disabledV: string = '0';
    public disabled: boolean = false;


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
    }

    public selectedServiceType(value: any): void {
        //console.log('ServiceType: ', value);
        this.serviceTypeCurrent = [value.id];
        if (this.serviceTypeCurrent[0] == 'Air' || this.serviceTypeCurrent[0] == 'Express') {
            this.cargoTypeCurrent = [];
        }
    }

    public selectedTypeClearance(value: any): void {
        //console.log('TypeClearance: ', value);
        this.typeClearanceCurrent = [value.id];
    }

    public selectedRouteClearance(value: any): void {
        //console.log('RouteClearance: ', value);
        this.routeClearanceCurrent = [value.id];
    }

    public selectedCargoType(value: any): void {
        //console.log('CargoType: ', value);
        this.cargoTypeCurrent = [value.id];
    }

    public removedServiceType(): void {
        this.serviceTypeCurrent = [];
    }

    public removedTypeClearance(): void {
        this.typeClearanceCurrent = [];
    }

    public removedRouteClearance(): void {
        this.routeClearanceCurrent = [];
    }

    public removedCargoType(): void {
        this.cargoTypeCurrent = [];
    }
}
