import { Component, OnInit } from '@angular/core';
import findIndex from 'lodash/findIndex';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { NgForm } from '@angular/forms';
import { CatChargeToAddOrUpdate } from 'src/app/shared/models/catalogue/catChargeToAddOrUpdate.model';
import { CatChargeDefaultAccount } from 'src/app/shared/models/catalogue/catChargeDefaultAccount.model';
import { Router } from '@angular/router';
import { ChargeConstants } from 'src/constants/charge.const';

@Component({
    selector: 'add-charge',
    templateUrl: './add-charge.component.html',
    styleUrls: ['./add-charge.component.scss']
})
export class AddChargeComponent implements OnInit {

    constructor(private baseServices: BaseService, private api_menu: API_MENU, private router: Router) { }
    ChargeToAdd: CatChargeToAddOrUpdate = new CatChargeToAddOrUpdate();
    isAddNewLine: boolean = false;
    isMaximumAccountRow: boolean = false;
    isSameVoucherType: boolean = false;
    ngDataUnit: any = [];
    ngDataCurrency: any = [];
    ngDataType = [
        { id: "CREDIT", text: "CREDIT" },
        { id: "DEBIT", text: "DEBIT" },
        { id: "OBH", text: "OBH" }
    ];
    ngDataTypeChargeDefault = [
        { id: "Công Nợ", text: "Công Nợ" },
        { id: "Giải Chi", text: "Giải Chi" },
        { id: "Loại Khác", text: "Loại Khác" }
    ];

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

    async ngOnInit() {
        await this.getNeccessaryData();

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
        if (this.ChargeToAdd.listChargeDefaultAccount.length === 0) {
            this.ChargeToAdd.listChargeDefaultAccount.push(obj);
        } else {
            if (this.validatateDefaultAcountLine()) {
                if (this.ChargeToAdd.listChargeDefaultAccount.length === 3) {
                    this.isMaximumAccountRow = true;
                } else {
                    this.ChargeToAdd.listChargeDefaultAccount.push(obj);
                    this.isAddNewLine = false;
                }
            } else {
                this.isAddNewLine = true;
            }

        }
    }

    RemoveDefaultAccount(index) {
        this.ChargeToAdd.listChargeDefaultAccount.splice(index, 1);
        this.isMaximumAccountRow = false;
    }

    async addCharge(form: NgForm) {
        console.log(form);
        console.log(this.ChargeToAdd);
        if (form.form.valid && this.validatateDefaultAcountLine()
            && this.isSameVoucherType === false
            && this.ChargeToAdd.charge.unitId != null
            && this.ChargeToAdd.charge.currencyId != null
            && this.ChargeToAdd.charge.type != null
            && (this.ChargeToAdd.charge.serviceTypeId != null && this.ChargeToAdd.charge.serviceTypeId !== '')) {
            delete this.ChargeToAdd.charge.id;
            const response = await this.baseServices.postAsync(this.api_menu.Catalogue.Charge.addNew, this.ChargeToAdd, true, true);
            if (response) {
                this.router.navigate(["/home/catalogue/charge"]);
            }
            console.log(this.ChargeToAdd);
        }

    }

    /**
   * ng2-select
   */

    private value: any = {};

    public selected(value: any, action): void {
        if (action === "unit") {
            this.ChargeToAdd.charge.unitId = value.id;

        }
        if (action === "currency") {
            this.ChargeToAdd.charge.currencyId = value.id;
        }
        if (action === "type") {
            this.ChargeToAdd.charge.type = value.id;
        }
        if (action === "service") {
            this.ChargeToAdd.charge.serviceTypeId = this.ChargeToAdd.charge.serviceTypeId === undefined ? (value.id + ";") : this.ChargeToAdd.charge.serviceTypeId += (value.id + ";");
        }
    }

    selectedTypeDefault(value: any, index: number) {
        const listAcc = this.ChargeToAdd.listChargeDefaultAccount;
        if (findIndex(listAcc, function (o) { return o.type === value.text; }) !== -1) {
            this.isSameVoucherType = true;
        } else {
            this.isSameVoucherType = false;
            this.ChargeToAdd.listChargeDefaultAccount[index].type = value.text;
        }

    }

    removedTypeDefault(value: any, index: number) {
        this.ChargeToAdd.listChargeDefaultAccount[index].type = null;
    }

    validatateDefaultAcountLine() {
        if (this.ChargeToAdd.listChargeDefaultAccount.length > 0) {
            const index = this.ChargeToAdd.listChargeDefaultAccount.length - 1;
            if ((this.ChargeToAdd.listChargeDefaultAccount[index].debitAccountNo === '' || this.ChargeToAdd.listChargeDefaultAccount[index].debitAccountNo == null)
                && (this.ChargeToAdd.listChargeDefaultAccount[index].creditAccountNo === '' || this.ChargeToAdd.listChargeDefaultAccount[index].creditAccountNo == null)
                && (this.ChargeToAdd.listChargeDefaultAccount[index].creditVat == null)
                && (this.ChargeToAdd.listChargeDefaultAccount[index].debitVat == null) || this.ChargeToAdd.listChargeDefaultAccount[index].type == null) {
                return false;
            } else {
                return true;
            }
        }

    }

    public removed(value: any, action: string): void {
        if (action === "service") {
            const s = value.id + ";";
            this.ChargeToAdd.charge.serviceTypeId = this.ChargeToAdd.charge.serviceTypeId.replace(s, "");
        }
        if (action === "unit") {
            this.ChargeToAdd.charge.unitId = null;
        }
        if (action === "currency") {
            this.ChargeToAdd.charge.currencyId = null;
        }
        if (action === "type") {
            this.ChargeToAdd.charge.type = null;
        }
    }

    public refreshValue(value: any): void {
        this.value = value;
    }
}
