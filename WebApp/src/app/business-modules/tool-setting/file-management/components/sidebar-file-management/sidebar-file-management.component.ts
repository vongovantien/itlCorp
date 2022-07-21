import { SystemConstants } from 'src/constants/system.const';
import { FormGroup } from '@angular/forms';
import { AbstractControl } from '@angular/forms';
import { FormBuilder } from '@angular/forms';
import { AppForm } from 'src/app/app.form';
import { AfterViewInit, Component, Input, OnInit, OnChanges, SimpleChanges, ViewChild, ElementRef, EventEmitter, Output } from '@angular/core';
import { ActivatedRoute, Event, NavigationEnd, Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { Renderer2 } from '@angular/core';
@Component({
    selector: 'sidebar-file-management',
    templateUrl: './sidebar-file-management.component.html',
    styleUrls: ['./sidebar-file-management.component.scss']
})
export class SidebarFileManagementComponent extends AppForm implements OnChanges, OnInit {
    @Input() isActiveSearch: boolean;
    @Input() listBreadcrumb: Array<object>;
    @Output() isDisplayDefaultFolder = new EventEmitter<string>();
    @Output() objectBack = new EventEmitter<any>();
    @Output() listKeySearch = new EventEmitter<any>();
    @Output() resetSearch = new EventEmitter<any>();

    title: string;
    formSearch: FormGroup;
    listKeyWord: AbstractControl;
    isActiveRouting: boolean;
    constructor(private _fb: FormBuilder, private route: ActivatedRoute, private _router: Router, private render: Renderer2) {
        super();
        this.requestReset = this.onResetSearch
    }

    ngOnInit(): void {
        this.initForm();
    }

    ngOnChanges(changes: SimpleChanges): void {
        this.title = this.route.snapshot.data['title']
        this.onActiveRouting(this.title);
        this.formSearch.reset()
    }

    initForm() {
        this.formSearch = this._fb.group({
            listKeyWord: []
        })
        this.listKeyWord = this.formSearch.controls['listKeyWord']
    }

    changeBreadcrumb() {
        this.title = this.route.snapshot.data['title']
    }

    navigateFileMngt(moduleUrl: string) {
        if (moduleUrl === 'accounting') {
            this._router.navigate([RoutingConstants.TOOL.FILE_MANAGMENT]);
        }
        else {
            this._router.navigate([RoutingConstants.TOOL.FILE_MANAGMENT + "/" + moduleUrl]);
        }
    }

    onBreadcrumbActive(item: any) {
        if (item === "Document" || item === "Accounting") {
            this.listBreadcrumb.splice(0, 3);
        } else {
            for (let index = 0; index < this.listBreadcrumb.length; index++) {
                if (item === this.listBreadcrumb[index]) {
                    this.listBreadcrumb.length = index + 1;
                }
            }
        }
        this.objectBack.emit(item)
    }

    onSubmitSearch() {
        let dataSearch = !!this.listKeyWord.value ? this.listKeyWord.value.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: any) => item.trim()) : null;
        this.listKeySearch.emit(dataSearch)
    }

    onResetSearch() {
        this.listKeySearch.emit([]);
    }

    onActiveRouting(event) {
        if (this.title === event) {
            return this.isActiveRouting = true;
        }
        return this.isActiveRouting = false;
    }
}
