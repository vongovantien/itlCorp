import { Component, OnInit } from '@angular/core';
import findIndex from 'lodash/findIndex';
import { BaseService } from 'src/app/shared/services/base.service';
import { ToastrService } from 'ngx-toastr';
import { API_MENU } from 'src/constants/api-menu.const';
import { NgForm } from '@angular/forms';
import { CatChargeToAddOrUpdate } from 'src/app/shared/models/catalogue/catChargeToAddOrUpdate.model';
import { CatChargeDefaultAccount } from 'src/app/shared/models/catalogue/catChargeDefaultAccount.model';
import { Router, ActivatedRoute } from '@angular/router';
import { ChargeConstants } from 'src/constants/charge.const';
@Component({
    selector: 'detail-charge',
    templateUrl: './detail-charge.component.html',
    styleUrls: ['./detail-charge.component.scss']
})
export class DetailChargeComponent implements OnInit {
    isAddNewLine: boolean = false;
    isMaximumAccountRow: boolean = false;
    isSameVoucherType: boolean = false;
    ngDataUnit: any = [];
    ngDataCurrency: any = [];
    Charge: CatChargeToAddOrUpdate = null;
    isSubmitted = false;
    ngDataType = [
        { id: "CREDIT", text: "CREDIT" },
        { id: "DEBIT", text: "DEBIT" },
        { id: "OBH", text: "OBH" }
    ];
    ngDataTypeChargeDefault = [
        { id: "Công-Nợ", text: "Công Nợ" },
        { id: "Giải-Chi", text: "Giải Chi" },
        { id: "Loại-Khác", text: "Loại Khác" }
    ];

    activeUnit: any = [];
    activeCurrency: any = [];
    activeType: any = [];
    activeServices: any = [];

    /**
     * Need to update ngDataServices by get data from databse after implement documentation module
     */
    ngDataService = [
        { text: ChargeConstants.IT_DES, id: ChargeConstants.IT_CODE },
        { text: ChargeConstants.AI_DES, id: ChargeConstants.AI_CODE },
        { text: ChargeConstants.AE_DES, id: ChargeConstants.AE_CODE },
        { text: ChargeConstants.SFE_DES, id: ChargeConstants.SFE_CODE },
        { text: ChargeConstants.SFI_DES, id: ChargeConstants.SFI_CODE },
        { text: ChargeConstants.SLE_DES, id: ChargeConstants.SLE_CODE },
        { text: ChargeConstants.SLI_DES, id: ChargeConstants.SLI_CODE },
        { text: ChargeConstants.SCE_DES, id: ChargeConstants.SCE_CODE },
        { text: ChargeConstants.SCI_DES, id: ChargeConstants.SCI_CODE },
        { text: ChargeConstants.CL_DES, id: ChargeConstants.CL_CODE }
    ];

    constructor(
        private baseServices: BaseService,
        private toastr: ToastrService,
        private api_menu: API_MENU,
        private route: ActivatedRoute,
        private router: Router) {
    }
    async ngOnInit() {
        await this.getNeccessaryData();
        await this.getChargeDetail();
    }

    async getChargeDetail() {
        try {
            this.route.params.subscribe(async params => {
                const id = params.id;
                this.Charge = await this.baseServices.getAsync(this.api_menu.Catalogue.Charge.getById + id, true, true);
                this.activeServices = this.getCurrentActiveService(this.Charge.charge.serviceTypeId);

                const idUnit = this.Charge.charge.unitId;
                const idCurrency = this.Charge.charge.currencyId;
                const type = this.Charge.charge.type;

                const indexCurrentUnit = findIndex(this.ngDataUnit, function (o) { return o['id'] === idUnit; });
                const indexCurrentCurrency = findIndex(this.ngDataCurrency, function (o) { return o['id'] === idCurrency; });
                const indexType = findIndex(this.ngDataType, function (o) { return o.id === type; });

                if (indexCurrentUnit > 0) {
                    this.activeUnit = [this.ngDataUnit[indexCurrentUnit]];
                }
                if (indexCurrentCurrency > 0) {
                    this.activeCurrency = [this.ngDataCurrency[indexCurrentCurrency]];
                }
                if (indexType > 0) {
                    this.activeType = [this.ngDataType[indexType]];
                }
            });
        } catch (error) {
            this.toastr.error("Cannot Get Charge Details !");
        }
    }

    getCurrentActiveService(ChargeService: any) {
        const listService = ChargeService.split(";");
        const activeServiceList: any = [];
        listService.forEach(item => {
            const index = findIndex(this.ngDataService, function (o) { return o.id === item; });
            if (index !== -1) {
                const activeService = this.ngDataService[index];
                activeServiceList.push(activeService);
            }
        });
        return activeServiceList;
    }

    async getNeccessaryData() {
        const units = await this.baseServices.getAsync(this.api_menu.Catalogue.Unit.getAll, false, false);
        this.ngDataUnit = units.length === 0 ? [] : units.map(x => ({ text: x.code, id: x.id }));

        const currencies = await this.baseServices.getAsync(this.api_menu.Catalogue.Currency.getAll, false, false);
        this.ngDataCurrency = currencies.length === 0 ? [] : currencies.map(x => ({ text: x.id + " - " + x.currencyName, id: x.id }));

        console.log({ unit: this.ngDataUnit, currency: this.ngDataCurrency });
    }

    addNewChargeDedaultAccount() {
        const obj = new CatChargeDefaultAccount();

        if (this.Charge.listChargeDefaultAccount.length === 0) {
            this.Charge.listChargeDefaultAccount.push(obj);
        } else {
            if (this.validatateDefaultAcountLine()) {
                if (this.Charge.listChargeDefaultAccount.length === 3) {
                    this.isMaximumAccountRow = true;
                } else {
                    this.Charge.listChargeDefaultAccount.push(obj);
                    this.isAddNewLine = false;
                }
            } else {
                this.isAddNewLine = true;
            }

        }
    }

    RemoveDefaultAccount(index) {
        this.Charge.listChargeDefaultAccount.splice(index, 1);
        this.isMaximumAccountRow = false;
    }

    async updateCharge(form: NgForm) {
        this.isAddNewLine = true;
        this.isSubmitted = true;
        if (form.form.status !== "INVALID" && this.validatateDefaultAcountLine()
            && this.isSameVoucherType === false
            && this.Charge.charge.unitId != null
            && this.Charge.charge.currencyId != null
            && this.Charge.charge.type != null
            && (this.Charge.charge.serviceTypeId != null && this.Charge.charge.serviceTypeId !== '')) {
            const response = await this.baseServices.putAsync(this.api_menu.Catalogue.Charge.update, this.Charge, true, true);
            if (response) {
                this.router.navigate(["/home/catalogue/charge"]);
            }
        }

    }


    getActiveVoucherType(index) {
        const voucherType = this.Charge.listChargeDefaultAccount[index].type;
        if (voucherType === null || voucherType === undefined) {
            return [];
        } else {
            const indexCurrenVoucher = findIndex(this.ngDataTypeChargeDefault, function (o) { return o.text === voucherType; });
            const aciveVoucherType = [this.ngDataTypeChargeDefault[indexCurrenVoucher]];
            return aciveVoucherType;
        }


    }

    /**
   * ng2-select
   */
    private value: any = {};
    public selected(value: any, action): void {
        if (action === "unit") {
            this.Charge.charge.unitId = value.id;

        }
        if (action === "currency") {
            this.Charge.charge.currencyId = value.id;
        }
        if (action === "type") {
            this.Charge.charge.type = value.id;
        }
        if (action === "service") {
            this.Charge.charge.serviceTypeId = this.Charge.charge.serviceTypeId === undefined ? (value.id + ";") : this.Charge.charge.serviceTypeId += (value.id + ";");
            console.log(this.Charge.charge.serviceTypeId);
        }
    }

    selectedTypeDefault(value: any, index: number) {
        const listAcc = this.Charge.listChargeDefaultAccount;
        if (findIndex(listAcc, function (o) { return o.type === value.text; }) !== -1) {
            this.isSameVoucherType = true;
        } else {
            this.isSameVoucherType = false;
            this.Charge.listChargeDefaultAccount[index].type = value.text;
        }

    }

    removedTypeDefault(value: any, index: number) {
        this.Charge.listChargeDefaultAccount[index].type = null;
    }

    validatateDefaultAcountLine() {
        if (this.Charge.listChargeDefaultAccount.length > 0) {
            const index = this.Charge.listChargeDefaultAccount.length - 1;
            if ((this.Charge.listChargeDefaultAccount[index].debitAccountNo === '' || this.Charge.listChargeDefaultAccount[index].debitAccountNo == null)
                && (this.Charge.listChargeDefaultAccount[index].creditAccountNo === '' || this.Charge.listChargeDefaultAccount[index].creditAccountNo == null)
                && (this.Charge.listChargeDefaultAccount[index].creditVat == null)
                && (this.Charge.listChargeDefaultAccount[index].debitVat == null) || this.Charge.listChargeDefaultAccount[index].type == null) {
                return false;
            } else {
                return true;
            }
        }

    }

    public removed(value: any, action): void {
        if (action === "service") {
            const s = value.id + ";";
            this.Charge.charge.serviceTypeId = this.Charge.charge.serviceTypeId.replace(s, "");
            console.log(this.Charge.charge.serviceTypeId);
        }
        if (action === "unit") {
            this.Charge.charge.unitId = null;
        }
        if (action === "currency") {
            this.Charge.charge.currencyId = null;
        }
        if (action === "type") {
            this.Charge.charge.type = null;
        }

        console.log('Removed value is: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }
}
