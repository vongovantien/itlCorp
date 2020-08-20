import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';

import { CommonEnum } from '@enums';
import { AccountReceivableListTrialOfficialComponent } from '../list-trial-official/list-trial-official-account-receivable.component';
import { AccountReceivableListGuaranteedComponent } from '../list-guaranteed/list-guaranteed-account-receivable.component';
import { AccountReceivableListOtherComponent } from '../list-other/list-other-account-receivable.component';
import { AccountReceivableFormSearchComponent } from '../form-search/account-receivable/form-search-account-receivable.component';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'tab-account-receivable',
    templateUrl: './tab-account-receivable.component.html',
})
export class AccountReceivableTabComponent extends AppList implements OnInit {

    //
    @ViewChild(AccountReceivableListTrialOfficialComponent, { static: false }) trialOfficalListComponent: AccountReceivableListTrialOfficialComponent;
    @ViewChild(AccountReceivableListGuaranteedComponent, { static: false }) guaranteedListComponent: AccountReceivableListGuaranteedComponent;
    @ViewChild(AccountReceivableListOtherComponent, { static: false }) otherListComponent: AccountReceivableListOtherComponent;
    //
    @ViewChild(AccountReceivableFormSearchComponent, { static: false }) accountReceivableFormComponent: AccountReceivableFormSearchComponent;

    selectedTab: string = "TRIAL_OFFICIAL";

    activeTrialOffice: boolean = false;
    activeGuaranteed: boolean = false;
    activeOther: boolean = false;

    constructor(
        private _router: Router,
        private _activeRouter: ActivatedRoute,
    ) {
        super();
    }
    ngOnInit() {
        this._activeRouter.queryParams.subscribe(param => {
            if (param.subTab === 'guaranteed') {
                this.changeActiveTabFlag(false, true, false);


            } else if (param.subTab === 'other') {
                this.changeActiveTabFlag(false, false, true);


            } else {
                this.changeActiveTabFlag(true, false, false);

            }

        });
    }
    //
    ngAfterViewInit() {
        this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical, this.trialOfficalListComponent);
    }
    //
    changeActiveTabFlag(trial_official: boolean = true, guaranteed: boolean = false, other: boolean = false) {
        this.activeTrialOffice = trial_official;
        this.activeGuaranteed = guaranteed;
        this.activeOther = other;
    }
    //
    onSearchReceivable(body: AccountingInterface.IAccReceivableSearch) {
        console.log("data search: ", body);

        switch (body.arType) {
            case CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical:
                this.setParameterToSearch(body, this.trialOfficalListComponent);
                break;
            case CommonEnum.TabTypeAccountReceivableEnum.Guarantee:
                this.setParameterToSearch(body, this.guaranteedListComponent);
                break;
            case CommonEnum.TabTypeAccountReceivableEnum.Other:
                this.setParameterToSearch(body, this.otherListComponent);
                break;
            default:
                break;
        }
    }
    //
    setParameterToSearch(dataSearch: AccountingInterface.IAccReceivableSearch, tabComponent: any) {
        tabComponent.dataSearch = dataSearch;
        tabComponent.getPagingList();
    }
    //
    onSelectTabAccountReceivable(tabname: string) {

        this.selectedTab = tabname;
        if (tabname === 'TRIAL_OFFICIAL') {
            this._router.navigate(['/home/accounting/account-receivable-payable'], { queryParams: { tab: 'receivable', subTab: 'trial_official' } });
            this.accountReceivableFormComponent.arType = CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical;
            this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical, this.trialOfficalListComponent);
        } else if (tabname === 'GUARANTEED') {
            this._router.navigate(['/home/accounting/account-receivable-payable'], { queryParams: { tab: 'receivable', subTab: 'guaranteed' } });
            this.accountReceivableFormComponent.arType = CommonEnum.TabTypeAccountReceivableEnum.Guarantee;
            this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.Guarantee, this.guaranteedListComponent);
        } else {

            this.accountReceivableFormComponent.arType = CommonEnum.TabTypeAccountReceivableEnum.Other;
            this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.Other, this.otherListComponent);
            this._router.navigate(['/home/accounting/account-receivable-payable'], { queryParams: { tab: 'receivable', subTab: 'other' } });
        }
        this.accountReceivableFormComponent.formSearch.patchValue(Object.assign({}));
        this.accountReceivableFormComponent.initForm();

    }

    setParameterToPagingTab(tab: CommonEnum.TabTypeAccountReceivableEnum, tabComponent: any) {
        this.accountReceivableFormComponent.arType = tab;
        //
        const dataSearch: AccountingInterface.IAccReceivableSearch = {
            arType: tab,
            acRefId: null,
            agreementStatus: null,
            agreementExpiredDay: 'All',
            overDueDay: 0,
            salesmanId: null,
            officeId: null,
        };
        tabComponent.dataSearch = dataSearch;
        tabComponent.getPagingList();
    }


}