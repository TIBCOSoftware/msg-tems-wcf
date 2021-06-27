﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: System.Runtime.Serialization.ContractNamespaceAttribute("http://www.tibco.com/BookOrderService", ClrNamespace = "www.tibco.com.BookOrderService")]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace="http://www.tibco.com/BookOrderService", ConfigurationName="BookOrderPT")]
public interface BookOrderPT
{

    // CODEGEN: Generating message contract since the operation orderBook is neither RPC nor document wrapped.
    [System.ServiceModel.OperationContractAttribute(Action = "\"/BookOrderService/BookOrderService.serviceagent/BookOrderPTEndpoint2/orderBook\"", ReplyAction = "*")]
    [System.ServiceModel.FaultContractAttribute(typeof(www.tibco.com.BookOrderService.orderBook_fault), Action = "orderBook", Name = "orderBook_fault")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    orderBookResponse1 orderBook(orderBookRequest1 request);
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.648")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.tibco.com/BookOrderService")]
public partial class orderBookRequest
{
    private string bookNameField;
    private int quantityField;
    private double priceField;
    private string purchaserField;
    private string creditCardNumField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
    public string bookName
    {
        get
        {
            return this.bookNameField;
        }
        set
        {
            this.bookNameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
    public int quantity
    {
        get
        {
            return this.quantityField;
        }
        set
        {
            this.quantityField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
    public double price
    {
        get
        {
            return this.priceField;
        }
        set
        {
            this.priceField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
    public string purchaser
    {
        get
        {
            return this.purchaserField;
        }
        set
        {
            this.purchaserField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
    public string creditCardNum
    {
        get
        {
            return this.creditCardNumField;
        }
        set
        {
            this.creditCardNumField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.648")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.tibco.com/BookOrderService")]
public partial class Book
{
    private string titleField;
    private string isbnField;
    private double priceField;
    private string[] authorsField;
    private string publisherField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
    public string title
    {
        get
        {
            return this.titleField;
        }
        set
        {
            this.titleField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
    public string isbn
    {
        get
        {
            return this.isbnField;
        }
        set
        {
            this.isbnField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
    public double price
    {
        get
        {
            return this.priceField;
        }
        set
        {
            this.priceField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(Order = 3)]
    [System.Xml.Serialization.XmlArrayItemAttribute("author", IsNullable = false)]
    public string[] authors
    {
        get
        {
            return this.authorsField;
        }
        set
        {
            this.authorsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
    public string publisher
    {
        get
        {
            return this.publisherField;
        }
        set
        {
            this.publisherField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.648")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.tibco.com/BookOrderService")]
public partial class ShippingDetails
{
    private string shipToField;
    private string shippingDateField;
    private string shippingAddressField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
    public string shipTo
    {
        get
        {
            return this.shipToField;
        }
        set
        {
            this.shipToField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
    public string shippingDate
    {
        get
        {
            return this.shippingDateField;
        }
        set
        {
            this.shippingDateField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
    public string shippingAddress
    {
        get
        {
            return this.shippingAddressField;
        }
        set
        {
            this.shippingAddressField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.648")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.tibco.com/BookOrderService")]
public partial class orderBookResponse
{
    private string orderNumberField;
    private ShippingDetails shippingDetailsField;
    private int quantityField;
    private double unitPriceField;
    private double discountField;
    private double orderTotalField;
    private Book bookField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
    public string orderNumber
    {
        get
        {
            return this.orderNumberField;
        }
        set
        {
            this.orderNumberField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
    public ShippingDetails ShippingDetails
    {
        get
        {
            return this.shippingDetailsField;
        }
        set
        {
            this.shippingDetailsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
    public int quantity
    {
        get
        {
            return this.quantityField;
        }
        set
        {
            this.quantityField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
    public double unitPrice
    {
        get
        {
            return this.unitPriceField;
        }
        set
        {
            this.unitPriceField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
    public double discount
    {
        get
        {
            return this.discountField;
        }
        set
        {
            this.discountField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
    public double orderTotal
    {
        get
        {
            return this.orderTotalField;
        }
        set
        {
            this.orderTotalField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
    public Book Book
    {
        get
        {
            return this.bookField;
        }
        set
        {
            this.bookField = value;
        }
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
public partial class orderBookRequest1
{
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.tibco.com/BookOrderService", Order = 0)]
    public orderBookRequest orderBookRequest;

    public orderBookRequest1()
    {
    }

    public orderBookRequest1(orderBookRequest orderBookRequest)
    {
        this.orderBookRequest = orderBookRequest;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
public partial class orderBookResponse1
{
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.tibco.com/BookOrderService", Order = 0)]
    public orderBookResponse orderBookResponse;

    public orderBookResponse1()
    {
    }

    public orderBookResponse1(orderBookResponse orderBookResponse)
    {
        this.orderBookResponse = orderBookResponse;
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public interface BookOrderPTChannel : BookOrderPT, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public partial class BookOrderPTClient : System.ServiceModel.ClientBase<BookOrderPT>, BookOrderPT
{
    public BookOrderPTClient()
    {
    }

    public BookOrderPTClient(string endpointConfigurationName) :
        base(endpointConfigurationName)
    {
    }

    public BookOrderPTClient(string endpointConfigurationName, string remoteAddress) :
        base(endpointConfigurationName, remoteAddress)
    {
    }

    public BookOrderPTClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
        base(endpointConfigurationName, remoteAddress)
    {
    }

    public BookOrderPTClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
        base(binding, remoteAddress)
    {
    }

    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    orderBookResponse1 BookOrderPT.orderBook(orderBookRequest1 request)
    {
        return base.Channel.orderBook(request);
    }

    public orderBookResponse orderBook(orderBookRequest orderBookRequest)
    {
        orderBookRequest1 inValue = new orderBookRequest1();
        inValue.orderBookRequest = orderBookRequest;
        orderBookResponse1 retVal = ((BookOrderPT)(this)).orderBook(inValue);
        return retVal.orderBookResponse;
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace = "http://www.tibco.com/BookOrderService", ConfigurationName = "BookSearchPT")]
public interface BookSearchPT
{
    // CODEGEN: Generating message contract since the wrapper name (searchBookRequest) of message searchBookRequest does not match the default value (searchBook)
    [System.ServiceModel.OperationContractAttribute(Action = "searchBook", ReplyAction = "*")]
    searchBookResponse searchBook(searchBookRequest request);
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName = "searchBookRequest", WrapperNamespace = "http://www.tibco.com/BookOrderService", IsWrapped = true)]
public partial class searchBookRequest
{
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.tibco.com/BookOrderService", Order = 0)]
    public string exactTitle;

    [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.tibco.com/BookOrderService", Order = 1)]
    public string titleSubstring;

    [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.tibco.com/BookOrderService", Order = 2)]
    public string authorSubstring;

    public searchBookRequest()
    {
    }

    public searchBookRequest(string exactTitle, string titleSubstring, string authorSubstring)
    {
        this.exactTitle = exactTitle;
        this.titleSubstring = titleSubstring;
        this.authorSubstring = authorSubstring;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName = "searchBookResponse", WrapperNamespace = "http://www.tibco.com/BookOrderService", IsWrapped = true)]
public partial class searchBookResponse
{
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.tibco.com/BookOrderService", Order = 0)]
    public www.tibco.com.BookOrderService.Books Books;

    public searchBookResponse()
    {
    }

    public searchBookResponse(www.tibco.com.BookOrderService.Books Books)
    {
        this.Books = Books;
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public interface BookSearchPTChannel : BookSearchPT, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public partial class BookSearchPTClient : System.ServiceModel.ClientBase<BookSearchPT>, BookSearchPT
{
    public BookSearchPTClient()
    {
    }

    public BookSearchPTClient(string endpointConfigurationName) :
        base(endpointConfigurationName)
    {
    }

    public BookSearchPTClient(string endpointConfigurationName, string remoteAddress) :
        base(endpointConfigurationName, remoteAddress)
    {
    }

    public BookSearchPTClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
        base(endpointConfigurationName, remoteAddress)
    {
    }

    public BookSearchPTClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
        base(binding, remoteAddress)
    {
    }

    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    searchBookResponse BookSearchPT.searchBook(searchBookRequest request)
    {
        return base.Channel.searchBook(request);
    }

    public www.tibco.com.BookOrderService.Books searchBook(string exactTitle, string titleSubstring, string authorSubstring)
    {
        searchBookRequest inValue = new searchBookRequest();
        inValue.exactTitle = exactTitle;
        inValue.titleSubstring = titleSubstring;
        inValue.authorSubstring = authorSubstring;
        searchBookResponse retVal = ((BookSearchPT)(this)).searchBook(inValue);
        return retVal.Books;
    }
}
namespace www.tibco.com.BookOrderService
{
    using System.Runtime.Serialization;

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "orderBook_fault", Namespace = "http://www.tibco.com/BookOrderService")]
    public partial class orderBook_fault : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        private string orderBook_faultMemberField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute(Name = "orderBook_fault", IsRequired = true, EmitDefaultValue = false)]
        public string orderBook_faultMember
        {
            get
            {
                return this.orderBook_faultMemberField;
            }
            set
            {
                this.orderBook_faultMemberField = value;
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "Books", Namespace = "http://www.tibco.com/BookOrderService", ItemName = "Book")]
    public class Books : System.Collections.Generic.List<www.tibco.com.BookOrderService.Book>
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "Book", Namespace = "http://www.tibco.com/BookOrderService")]
    public partial class Book : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        private string titleField;
        private string isbnField;
        private double priceField;
        private www.tibco.com.BookOrderService.Book.authorsType authorsField;
        private string publisherField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, EmitDefaultValue = false)]
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, EmitDefaultValue = false, Order = 1)]
        public string isbn
        {
            get
            {
                return this.isbnField;
            }
            set
            {
                this.isbnField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 2)]
        public double price
        {
            get
            {
                return this.priceField;
            }
            set
            {
                this.priceField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, EmitDefaultValue = false, Order = 3)]
        public www.tibco.com.BookOrderService.Book.authorsType authors
        {
            get
            {
                return this.authorsField;
            }
            set
            {
                this.authorsField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, EmitDefaultValue = false, Order = 4)]
        public string publisher
        {
            get
            {
                return this.publisherField;
            }
            set
            {
                this.publisherField = value;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
        [System.Runtime.Serialization.CollectionDataContractAttribute(Name = "Book.authorsType", Namespace = "http://www.tibco.com/BookOrderService", ItemName = "author")]
        public class authorsType : System.Collections.Generic.List<string>
        {
        }
    }
}