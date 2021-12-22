import { Component, OnInit, ViewChild } from '@angular/core';
import { RuleLinkFee } from 'src/app/shared/models/tool-setting/rule-link-fee';
import { SettingRepo } from 'src/app/shared/repositories';
import { map, catchError, finalize } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from 'src/app/shared/services';
import { ConfirmPopupComponent, Permission403PopupComponent } from 'src/app/shared/common/popup';
import { ToastrService } from 'ngx-toastr';
import { AppList } from 'src/app/app.list';
import { FormRuleComponent } from './components/form-rule/form-rule.component';
@Component({
  selector: 'app-link-fee',
  templateUrl: './link-fee.component.html',
})
export class LinkFeeComponent extends AppList implements OnInit{
  @ViewChild(FormRuleComponent) formRule: FormRuleComponent;
  @ViewChild(ConfirmPopupComponent) confirmDeletePopup: ConfirmPopupComponent;
  @ViewChild(Permission403PopupComponent) permission403Popup: Permission403PopupComponent;

  rules: RuleLinkFee[] = [];
  headers: CommonInterface.IHeaderTable[] = [];

  indexRuleToDelete: string = '';
  constructor(
    private _settingRepo: SettingRepo,
    private _progressService: NgProgress,
    private _toastService: ToastrService,
  ) {
    super();
    this._progressRef = this._progressService.ref();
    this.requestList = this.searchRule;
  }

  ngOnInit() {
    this.headers = [
      { field: 'ruleName', title: 'Name Rule', sortable: false },
      { field: 'serviceBuying', title: 'Service Buying', sortable: false },
      { field: 'chargeNameBuying', title: 'Charge Buying', sortable: false },
      { field: 'partnerNameBuying', title: 'Partner Buying', sortable: false },
      { field: 'serviceSelling', title: 'Service Selling', sortable: false },
      { field: 'chargeNameSelling', title: 'Charge Selling', sortable: false },
      { field: 'partnerSelling', title: 'Partner Selling', sortable: false },
      { field: 'userNameCreated', title: 'Creator', sortable: false },
      { field: 'modifiedDate', title: 'Modified Date', sortable: false },
      { field: 'status', title: 'Status', sortable: false },
      { field: 'effectiveDate', title: 'Effective Date', sortable: false },
      { field: 'expiredDate', title: 'Expiration Date', sortable: false },
    ];
    this.dataSearch = {
      all: null
    };
    this.searchRule(this.dataSearch);
  }

  searchRule(dataSearch?: any) {
    this.isLoading = true;
    this._progressRef.start();
    this._settingRepo.getRule(this.page, this.pageSize, Object.assign({}, dataSearch))
      .pipe(
        catchError(this.catchError),
        finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
        map((data: any) => {
          return {
            data: (data.data || []).map((item: RuleLinkFee) => new RuleLinkFee(item)),
            totalItems: data.totalItems,
          };
        })
      ).subscribe(
        (res: any) => {
          this.totalItems = res.totalItems || 0;
          this.rules=res.data.forEach(element => {
            element.serviceBuying=this.convertServiceName(element.serviceBuying),
            element.serviceSelling=this.convertServiceName(element.serviceSelling)
          });
          this.rules = res.data || [];
        },
      );
  }


  deleteRule(rule: RuleLinkFee) {
    this._progressRef.start();

    this.indexRuleToDelete = rule.id.toUpperCase();
    this.confirmDeletePopup.show();
  }

  onDeleteRule() {

    this._settingRepo.deleteRule(this.indexRuleToDelete)
      .pipe(
        finalize(() => { this._progressRef.complete(); this.confirmDeletePopup.hide(); })
      )
      .subscribe(
        (res: CommonInterface.IResult) => {
          if (res.status) {
            this._toastService.success(res.message);
            this.resetSearch();
          } else {
            this._toastService.error(res.message);
          }
        });
  }

  convertServiceName(serviceCode: string) {
    switch (serviceCode) {
      case 'AI':
        return 'Air Import';
      case 'CL':
        return 'Custom Logistic';
      case 'AE':
        return 'Air Export';
      case 'SFE':
        return 'Sea FCL Export';
      case 'SFI':
        return 'Sea FCL Import';
      case 'SLE':
        return 'Sea LCL Export';
      case 'SLI':
        return 'Sea LCL Import';
      case 'IT':
        return 'Inland Trucking';
      case 'SCE':
        return 'Sea Consol Export';
      case 'SCI':
        return 'Sea Consol Import';
      default:
        break;
    }
  }

  getRuleDetail(id: string) {
    this._settingRepo.getDetailRule(id)
      .pipe()
      .subscribe(
        (res: RuleLinkFee) => {
          if (!!res) {
            this.formRule.formGroup.patchValue(res);
            this.formRule.title = 'Detail/Edit Rule';
            this.formRule.rule.id = id;
            this.formRule.datetimeCreated = res.datetimeCreated;
            this.formRule.userNameCreated = res.userNameCreated;
            this.formRule.datetimeModified = res.datetimeModified;
            this.formRule.userNameModified = res.userNameModified;
            this.formRule.rule.expiredDate = res.expiredDate;
            this.formRule.rule.effectiveDate = res.effectiveDate;
            this.formRule.rule.partnerBuying = res.partnerBuying;
            this.formRule.rule.partnerSelling = res.partnerSelling;
            this.formRule.rule.chargeBuying = res.chargeBuying;
            this.formRule.rule.chargeSelling = res.chargeSelling;
            this.formRule.expiredDate.setValue({ startDate: res.expiredDate?new Date(res.expiredDate):null });
            this.formRule.effectiveDate.setValue({ startDate: res.effectiveDate? new Date(res.effectiveDate):null });
            let chargeBuying = this.formRule.configChargeBuying.dataSource.find(
              x => x.id === res.chargeBuying
            );
            let chargeSelling = this.formRule.configChargeSelling.dataSource.find(
              x => x.id === res.chargeSelling
            );
            let partnerBuying = this.formRule.configPartner.dataSource.find(
              x => x.id === res.partnerBuying
            );
            let partnerSelling = this.formRule.configPartner.dataSource.find(
              x => x.id === res.partnerSelling
            );
            this.formRule.selectedChargeBuying =
              { field: 'chargeNameEn', value: chargeBuying.chargeNameEn, data: chargeBuying };
            this.formRule.selectedChargeSelling =
              { field: 'chargeNameEn', value: chargeSelling.chargeNameEn, data: chargeSelling };
            this.formRule.selectedPartnerBuying =
              { field: 'shortName', value: partnerBuying.shortName, data: partnerBuying };
            this.formRule.selectedPartnerSelling =
              { field: 'shortName', value: partnerSelling.shortName, data: partnerSelling };
            this.formRule.isShowUpdate = true;
            this.formRule.show();
          }
        }
      );
  }

  showCreateRule() {
    this.formRule.title = 'Add new Rule Link Fee';
    this.formRule.formGroup.reset();
    this.formRule.selectedChargeBuying = { field: 'chargeNameEn', value: null, data: null };
    this.formRule.selectedChargeSelling = { field: 'chargeNameEn', value: null, data: null };
    this.formRule.selectedPartnerBuying = { field: 'shortName', value: null, data: null };
    this.formRule.selectedPartnerSelling = { field: 'shortName', value: null, data: null };
    this.formRule.isBuying = true;
    this.formRule.isSelling = true;
    this.formRule.isShowUpdate = false;
    this.formRule.show();
  }

  resetSearch() {
    this.dataSearch = {};
    this.page = 1;
    this.searchRule(this.dataSearch);
  }

}