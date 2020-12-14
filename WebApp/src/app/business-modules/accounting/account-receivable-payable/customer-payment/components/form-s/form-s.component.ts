import { formatDate } from '@angular/common';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { AccountingConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Currency, Customer, User } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { AppForm } from 'src/app/app.form';

@Component({
  selector: 'app-form-s',
  templateUrl: './form-s.component.html',
})
export class FormSComponent extends AppForm implements OnInit {
  @Output() onSearch: EventEmitter<IAcctReceiptCriteria> = new EventEmitter<IAcctReceiptCriteria>();
  @Output() onReset: EventEmitter<IAcctReceiptCriteria> = new EventEmitter<IAcctReceiptCriteria>();

  customerIDs: Observable<Customer[]>;
  creators: Observable<User[]>;
  customerID: AbstractControl;
  creator: AbstractControl;
  refNo: AbstractControl;
  paymentTypes: string[] = AccountingConstants.PAYMENT_TYPE;
  paymentType: AbstractControl;
  displayFilesPartners: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
  date: AbstractControl;
  dateTypes: string[] = AccountingConstants.DATE_TYPE;
  dateType: AbstractControl;
  Currencys: Observable<Currency[]>;
  currency: AbstractControl;
  syncStatuss = AccountingConstants.SYNC_STATUS;
  syncStatus: AbstractControl;
  statuss = AccountingConstants.STATUS;
  status: AbstractControl;
  formSearch: FormGroup;


  constructor(
    private _catalogueRepo: CatalogueRepo,
    private _systemRepo: SystemRepo,
    private _fb: FormBuilder,
  ) {
    super();
    this.requestReset = this.requestSearch;
  }

  ngOnInit() {
    this.initForm();
    this.customerIDs = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL, null);
    this.creators = this._systemRepo.getSystemUsers();
  }
  initForm() {
    this.formSearch = this._fb.group({
      refNo: [],
      paymentType: [this.paymentType[0]],
      customerID: [],
      date: [{ startDate: new Date, endDate: new Date }],
      dateType: [this.dateType[0]],
      currency: [],
      syncStatus: [],
      status: [],
    });
    this.refNo = this.formSearch.controls['refNo'];
    this.paymentType = this.formSearch.controls['paymentType'];
    this.customerID = this.formSearch.controls['customerID'];
    this.date = this.formSearch.controls['date'];
    this.dateType = this.formSearch.controls['dateType'];
    this.currency = this.formSearch.controls['currency'];
    this.syncStatus = this.formSearch.controls['syncStatus'];
    this.status = this.formSearch.controls['status'];
  }
  onSelectDataFormInfo(data: any, type: string) {
    switch (type) {
      case 'partner':
        this.formSearch.controls['partner'].setValue(data.id);
        break;
      case 'currency':
        this.formSearch.controls['currency'].setValue(data.id);
        break;
      default:
        break;

    }

  }

  search() {
    const body: IAcctReceiptCriteria = {
      refNo: this.refNo.value,
      paymentType: this.paymentType.value === 'All' ? this.paymentType.value : null,
      customerID: this.customerID.value,
      dateFrom: (!!this.date.value && !!this.date.value.startDate) ? formatDate(this.date.value.startDate, 'yyyy-MM-dd', 'en') : null,
      dateTo: (!!this.date.value && !!this.dataReport.value.endDate) ? formatDate(this.date.value.endDate, 'yyyy-MM-dd', 'en') : null,
      dateType: this.dateType.value === 'All' ? this.dateType.value : null,
      currency: this.currency.value,
      syncStatus: this.syncStatus.value,
      status: this.status.value === 'All' ? this.status.value : null,
    };
  }
  reset() {
    this.resetKeywordSearchCombogrid();
    this.formSearch.reset();
    this.resetFormControl(this.customerID);
    this.onReset.emit(<any>{ transactionType: null });
  }
}
interface IAcctReceiptCriteria {
  refNo: string;
  paymentType: string;
  customerID: string;
  dateFrom: string;
  dateTo: string;
  dateType: string;
  currency: string;
  syncStatus: string;
  status: string;
}
