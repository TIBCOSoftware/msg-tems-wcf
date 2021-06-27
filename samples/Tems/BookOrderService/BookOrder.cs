/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

using System.Runtime.Serialization;
using System.ServiceModel;

namespace www.tibco.com.BookOrderService
{
    [MessageContract(WrapperName = "orderBookRequest", WrapperNamespace = "http://www.tibco.com/BookOrderService", IsWrapped = true)]
    public class orderBookRequest
    {
        private string bookNameField;
        private int quantityField;
        private double priceField;
        private string purchaserField;
        private string creditCardNumField;

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 0)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 1)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 2)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 3)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 4)]
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

    [MessageContract(WrapperName = "orderBookResponse", WrapperNamespace = "http://www.tibco.com/BookOrderService", IsWrapped = true)]
    public class orderBookResponse
    {
        private string orderNumberField;
        private ShippingDetails shippingDetailsField;
        private int quantityField;
        private double unitPriceField;
        private double discountField;
        private double orderTotalField;
        private Book bookField;

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 0)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 1)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 2)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 3)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 4)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 5)]
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

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 6)]
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

    [MessageContract(WrapperName = "searchBookRequest", WrapperNamespace = "http://www.tibco.com/BookOrderService", IsWrapped = true)]
    public class searchBookRequest
    {
        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 0)]
        public string exactTitle;

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 1)]
        public string titleSubstring;

        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 2)]
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

    [MessageContract(WrapperName = "searchBookResponse", WrapperNamespace = "http://www.tibco.com/BookOrderService", IsWrapped = true)]
    public class searchBookResponse
    {
        [MessageBodyMember(Namespace = "http://www.tibco.com/BookOrderService", Order = 0)]
        public Books Books;

        public searchBookResponse()
        {
        }

        public searchBookResponse(Books Books)
        {
            this.Books = Books;
        }
    }

    [DataContract(Name = "orderBook_fault", Namespace = "http://www.tibco.com/BookOrderService")]
    public class orderBook_fault : object, IExtensibleDataObject
    {
        private ExtensionDataObject extensionDataField;
        private string orderBook_faultMemberField;

        public ExtensionDataObject ExtensionData
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

        [DataMember(Name = "orderBook_fault", IsRequired = true, EmitDefaultValue = false)]
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

    [CollectionDataContract(Namespace = "http://www.tibco.com/BookOrderService", ItemName = "Book")]
    public class Books : System.Collections.Generic.List<Book>
    {
    }

    [CollectionDataContract(Namespace = "http://www.tibco.com/BookOrderService", ItemName = "author")]
    public class ArrayOfString : System.Collections.Generic.List<string>
    {
    }

    [DataContract(Name = "Book", Namespace = "http://www.tibco.com/BookOrderService")]
    public class Book
    {
        private string titleField;
        private string isbnField;
        private double priceField;
        private ArrayOfString authorsField;
        private string publisherField;

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
        public ArrayOfString authors
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

        [DataMember]
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

    [DataContract(Name = "ShippingDetails", Namespace = "http://www.tibco.com/BookOrderService")]
    public class ShippingDetails
    {
        private string shipToField;
        private string shippingDateField;
        private string shippingAddressField;

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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
}
