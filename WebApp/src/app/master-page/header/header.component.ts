import { Component, OnInit, AfterViewInit, Output, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute, NavigationStart, NavigationEnd, NavigationCancel, NavigationError, Event } from '@angular/router';
import { IdentityRepo, SystemRepo } from '@repositories';

import { SystemConstants } from 'src/constants/system.const';
import { Employee, Office, SysUserNotification } from '@models';
import { forkJoin, Subscription } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { GlobalState } from 'src/app/global-state';
import { SignalRService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, AfterViewInit {

    @Output() officeChange: EventEmitter<Office> = new EventEmitter<Office>();
    @Output() groupDepartmentChange: EventEmitter<SystemInterface.IDepartmentGroup> = new EventEmitter<SystemInterface.IDepartmentGroup>();


    english_flag = "assets/app/media/img/lang/en.png";
    vietnam_flag = "assets/app/media/img/lang/vi.png";
    active_flag = "assets/app/media/img/lang/en.png";

    currenUser: SystemInterface.IClaimUser;

    offices: Office[];
    selectedOffice: Office;

    departmentGroups: SystemInterface.IDepartmentGroup[] = [];
    selectedDepartmentGroup: SystemInterface.IDepartmentGroup;
    pageTitle: string = 'Home';

    subscriptions: Subscription[] = [];

    notifications: SysUserNotification[] = [];
    newMssUnread: number;

    spinnerNotify: string = "spinnerNotify";
    page: number = 1;
    size: number = 10;
    totalItem: number = 0;

    constructor(
        private router: Router,
        private _systemRepo: SystemRepo,
        private _activedRouter: ActivatedRoute,
        private _identity: IdentityRepo,
        private _globalState: GlobalState,
        private _signalRService: SignalRService,
        private _toast: ToastrService,
        private _spinner: NgxSpinnerService
    ) { }

    ngOnInit() {
        this.currenUser = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));

        if (!!this.currenUser) {
            this.getOfficeDepartGroupCurrentUser(this.currenUser);
        }


        const indentitySub = this._identity.getUserProfile()
            .subscribe(
                (user: any) => {
                    if (!!user) {
                        this.currenUser.photo = user.photo;
                        this.currenUser.nameEn = user.nameEn;
                        this.currenUser.title = user.title;

                    }
                }
            );

        const routerSub = this.router.events
            .subscribe((event: Event) => {
                switch (true) {
                    case event instanceof NavigationEnd:
                        const data = this.createBreadcrumbs(this._activedRouter.root);
                        if (!!data.length && data.length > 2) {
                            this.pageTitle = data[2].name;
                        } else {
                            this.pageTitle = "Home";
                        }
                        break;
                    case event instanceof NavigationStart:
                    case event instanceof NavigationCancel:
                    case event instanceof NavigationError: {
                        break;
                    }
                    default: {
                        break;
                    }
                }
            });

        // * Subscribe data profile change.
        this._globalState.subscribe('profile', (data: Employee) => {
            if (!!data) {
                this.currenUser.photo = data.photo;
                this.currenUser.nameEn = data.employeeNameEn;
                this.currenUser.title = data.title;
            }
        });

        this.subscriptions.push(indentitySub, routerSub);
        // * Realtime
        this._signalRService.startConnection();
        this.getListNotification();

        this._signalRService.listenEvent("NotificationWhenChange", (data: SysUserNotification) => {
            if (data) {
                // this._toast.info(`You have a new message ${data.title}`, 'Infomation');
                this.getListNotification();
            }
        });
    }

    getListNotification() {
        this._systemRepo.getListNotifications()
            .pipe()
            .subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.notifications = res.data || [];
                    this.totalItem = res.totalItems;

                    this.newMssUnread = this.notifications.filter(x => x.status === 'New').length;
                }
            );
    }

    selectNotification(noti: SysUserNotification, e: any) {
        e.stopPropagation();
        if (noti.status === 'Read') {
            return;
        }
        this._spinner.show(this.spinnerNotify);
        this._systemRepo.readMessage(noti.id)
            .pipe(
                finalize(() => {
                    this._spinner.hide(this.spinnerNotify);
                })
            )
            .subscribe((res: CommonInterface.IResult) => {
                if (!res.status) {
                    this._toast.error("Có lỗi xảy ra, vui lòng kiểm tra lại tin nhắn");

                } else {
                    noti.status = 'Read';
                    this.newMssUnread = this.notifications.filter(x => x.status === 'New').length;
                }

            });
    }

    loadMoreNotification(e: any) {
        e.stopPropagation();
        this._spinner.show(this.spinnerNotify);
        this.page++;

        this._systemRepo.getListNotifications(this.page, this.size)
            .pipe(
                finalize(() => {
                    this._spinner.hide(this.spinnerNotify);
                })
            )
            .subscribe((res: CommonInterface.IResponsePaging) => {
                if (!!res.data) {
                    this.notifications = [...this.notifications, ...res.data];
                    this.totalItem = res.totalItems;
                }
            });

    }

    viewProfile() {
        this.router.navigate([`home/profile/${this.currenUser.id}`]);
    }

    getOfficeDepartGroupCurrentUser(currenUser: SystemInterface.IClaimUser) {
        forkJoin([
            this._systemRepo.getOfficePermission(currenUser.id, currenUser.companyId),
            this._systemRepo.getDepartmentGroupPermission(currenUser.id, currenUser.officeId)
        ]).pipe(tap((res: any) => {
            this.offices = res[0] || [];
            if (!!this.offices.length) {
                if (this.offices.length === 1) {
                    this.selectedOffice = this.offices[0];
                } else {
                    this.selectedOffice = this.offices.find(o => o.id === currenUser.officeId);
                }

                // * SAVE USER LOGGED OFFICE'S 
                localStorage.setItem(SystemConstants.CURRENT_OFFICE, this.selectedOffice.branchNameEn);
            }
        })).subscribe((res: any) => {
            if (!!res) {
                this.departmentGroups = res[1] || [];
                if (!!this.departmentGroups.length) {
                    let dept = +currenUser.departmentId;
                    if (!dept) {
                        dept = null;
                    }
                    if (this.departmentGroups.length === 1) {
                        this.selectedDepartmentGroup = this.departmentGroups[0];
                    } else {
                        this.selectedDepartmentGroup = this.departmentGroups.find(d => d.departmentId === dept && d.groupId === +currenUser.groupId);
                    }
                }
            }
        });
    }

    createBreadcrumbs(route: ActivatedRoute, path: string = '', breadcrumbs: any[] = []): any[] {
        const children: ActivatedRoute[] = route.children;

        if (children.length === 0) {
            return breadcrumbs;
        }

        for (const child of children) {
            const routeURL: string = child.snapshot.url.map(segment => segment.path).join('/');
            if (routeURL !== '') {
                path += decodeURI(`/${routeURL}`);
            }

            const label = child.snapshot.data['name'];
            if (!!(label)) {
                breadcrumbs.push({
                    name: label,
                    path: path
                });
            }
            return this.createBreadcrumbs(child, path, breadcrumbs);
        }
    }

    changeOffice(office: Office) {
        if (!!office && this.offices.length > 1) {
            this.officeChange.emit(office);
        }
    }

    changeDepartmentGroup(departmentGroup: SystemInterface.IDepartmentGroup) {
        if (!!departmentGroup && this.departmentGroups.length > 1) {
            this.groupDepartmentChange.emit(departmentGroup);
        }
    }

    getCurrentLangFromUrl() {
        const url = window.location.href;
        const host = window.location.hostname;
        const url_arr = url.split("/");
        const current_lang_index = url_arr.indexOf(host) + 1;
        return url_arr[current_lang_index];
    }

    ngAfterViewInit(): void {
        setTimeout(() => {
            const currentLang = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);
            if (!!currentLang && currentLang === SystemConstants.LANGUAGES.ENGLISH) {
                this.active_flag = this.english_flag;
            }
            if (!!currentLang && currentLang === SystemConstants.LANGUAGES.VIETNAM) {
                this.active_flag = this.vietnam_flag;
            }
        }, 100);

    }

    changeLanguage(lang: string) {
        if (lang === localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE)) {
            return;
        } else {
            if (lang === SystemConstants.LANGUAGES.ENGLISH) {
                localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, SystemConstants.LANGUAGES.ENGLISH);
                localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, SystemConstants.LANGUAGES.ENGLISH_API);
                const url = window.location.protocol + "//" + window.location.hostname + "/" + lang + "/#" + this.router.url + "/";
                this.active_flag = this.english_flag;
                window.location.href = url;
            }
            if (lang === SystemConstants.LANGUAGES.VIETNAM) {
                localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, SystemConstants.LANGUAGES.VIETNAM);
                localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, SystemConstants.LANGUAGES.VIETNAM_API);
                const url = window.location.protocol + "//" + window.location.hostname + "/" + lang + "/#" + this.router.url + "/";
                this.active_flag = this.vietnam_flag;
                window.location.href = url;
            }
        }
    }

    // minimize sidebar
    minimize_page_sidebar_desktop() {
        const bodyElement = document.getElementById('bodyElement');
        const leftMinimizeToggle = document.getElementById('m_aside_left_minimize_toggle');
        if (leftMinimizeToggle.classList.contains('m-brand__toggler--active')) {
            bodyElement.classList.remove('m-brand--minimize', 'm-aside-left--minimize');
            leftMinimizeToggle.classList.remove('m-brand__toggler--active');
        } else {
            bodyElement.classList.add('m-brand--minimize', 'm-aside-left--minimize');
            leftMinimizeToggle.classList.add('m-brand__toggler--active');
        }
    }

    minimize_page_sidebar_mobile() {
        const leftMinimizeToggle = document.getElementById('m_aside_left');

        if (leftMinimizeToggle.classList.contains('m-aside-mobile--active')) {
            leftMinimizeToggle.classList.remove('m-aside-mobile--active');
        } else {
            leftMinimizeToggle.classList.add('m-aside-mobile--active');

        }
    }

    ngOnDestroy(): void {
        if (this.subscriptions.length) {
            this.subscriptions.forEach(c => c.unsubscribe());
        }
    }

}


