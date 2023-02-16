import { Component, EventEmitter, Input, OnChanges, OnInit, Output, Renderer2 } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AppForm } from 'src/app/app.form';
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


    @Output() directTo: EventEmitter<string> = new EventEmitter<string>();

    title: string = 'Accounting';
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

    initForm() {
        this.formSearch = this._fb.group({
            listKeyWord: []
        })
        this.listKeyWord = this.formSearch.controls['listKeyWord']
    }

    changeBreadcrumb() {
        this.title = this.route.snapshot.data['title']
    }

    redirectTo(type: string) {
        this.directTo.emit(type);
        this.title = type;
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

    onResetSearch() {
        this.listKeySearch.emit([]);
    }
}
